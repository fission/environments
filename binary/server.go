package main

import (
	"context"
	"encoding/json"
	"flag"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"os/exec"
	"os/signal"
	"path/filepath"
	"strings"
	"sync"
	"syscall"
)

const (
	DEFAULT_CODE_PATH          = "/userfunc/user"
	DEFAULT_INTERNAL_CODE_PATH = "/bin/userfunc"
)

var specialized bool

type (
	BinaryServer struct {
		fetchedCodePath  string
		internalCodePath string
	}

	FunctionLoadRequest struct {
		// FilePath is an absolute filesystem path to the
		// function. What exactly is stored here is
		// env-specific. Optional.
		FilePath string `json:"filepath"`

		// FunctionName has an environment-specific meaning;
		// usually, it defines a function within a module
		// containing multiple functions. Optional; default is
		// environment-specific.
		FunctionName string `json:"functionName"`

		// URL to expose this function at. Optional; defaults
		// to "/".
		URL string `json:"url"`
	}
)

func HttpResponse(w http.ResponseWriter, status int, body []byte) {
	w.WriteHeader(status)
	_, err := w.Write(body)
	if err != nil {
		log.Printf("Failed to write response: %s\n", err)
	}
}

func HttpResponseWithError(w http.ResponseWriter, status int, err error) {
	log.Println("Error:", err)
	HttpResponse(w, status, []byte(err.Error()))
}

func (bs *BinaryServer) SpecializeHandler(w http.ResponseWriter, r *http.Request) {
	log.Println("Starting Specialize request")
	if specialized {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("already specialized"))
		return
	}

	request := FunctionLoadRequest{}
	err := json.NewDecoder(r.Body).Decode(&request)
	if err != io.EOF && err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to parse request: %w", err))
		return
	}

	log.Printf("Decoded function load request: %#v\n", request)
	codePath := bs.fetchedCodePath

	if request.FilePath != "" {
		fileStat, err := os.Stat(request.FilePath)
		if err != nil {
			HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to stat file: %w", err))
			return
		}

		codePath = request.FilePath
		switch mode := fileStat.Mode(); {
		case mode.IsDir():
			codePath = filepath.Join(request.FilePath, request.FunctionName)
		}
	}

	fileStat, err := os.Stat(codePath)
	if err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to stat file: %w", err))
		return
	}
	if !fileStat.Mode().IsRegular() {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("file is not a regular file: %s", codePath))
		return
	}
	// Future: Check if executable is correct architecture/executable.

	// Copy the executable to ensure that file is executable and immutable.
	userFunc, err := os.ReadFile(codePath)
	if err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to read file: %w", err))
		return
	}
	err = os.WriteFile(bs.internalCodePath, userFunc, 0555)
	if err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to write file: %w", err))
		return
	}
	bs.internalCodePath = codePath
	log.Printf("BinaryServer: %#v\n", bs)
	specialized = true
	log.Println("done specializing")
}

func (bs *BinaryServer) InvocationHandler(w http.ResponseWriter, r *http.Request) {
	if !specialized {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("not specialized"))
		return
	}
	log.Println("Starting binary function execution")

	// CGI-like passing of environment variables
	execEnv := NewEnv(nil)
	execEnv.SetEnv(&EnvVar{"REQUEST_METHOD", r.Method})
	execEnv.SetEnv(&EnvVar{"REQUEST_URI", r.RequestURI})
	execEnv.SetEnv(&EnvVar{"CONTENT_LENGTH", fmt.Sprintf("%d", r.ContentLength)})
	execEnv.SetEnv(&EnvVar{"PATH", "$PATH:/bin:/usr/bin:/usr/local/bin:/userfunc/deployarchive:/userfunc"})

	for header, val := range r.Header {
		execEnv.SetEnv(&EnvVar{fmt.Sprintf("HTTP_%s", strings.ToUpper(header)), val[0]})
	}

	// Future: could be improved by keeping subprocess open while environment is specialized
	cmd := exec.Command("/bin/sh", "-c", bs.internalCodePath)
	cmd.Env = execEnv.ToStringEnv()

	stdinPipe, err := cmd.StdinPipe()
	if err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to get stdin pipe: %w", err))
		return
	}
	stderrPipe, err := cmd.StderrPipe()
	if err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to get stderr pipe: %w", err))
		return
	}
	stdoutPipe, err := cmd.StdoutPipe()
	if err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to get stdout pipe: %w", err))
		return
	}

	err = cmd.Start()
	if err != nil {
		HttpResponseWithError(w, http.StatusInternalServerError, fmt.Errorf("failed to start subprocess: %w", err))
		return
	}

	wg := sync.WaitGroup{}
	wg.Add(1)

	go func() {
		defer func() {
			stdinPipe.Close()
			wg.Done()
		}()
		if r.ContentLength == 0 {
			return
		}
		written, err := io.Copy(stdinPipe, r.Body)
		if err != nil {
			HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to copy body to stdin: %w", err))
			return
		}
		log.Printf("ContentLength is %d. Wrote %d bytes to stdin\n", r.ContentLength, written)
	}()

	wg.Add(1)
	go func() {
		defer wg.Done()
		stderr, err := io.ReadAll(stderrPipe)
		if err != nil {
			HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to get stderr pipe: %w", err))
			return
		}
		if len(stderr) > 0 {
			log.Printf("stderr: %s\n", stderr)
		}
	}()

	wg.Add(1)
	var stdout []byte
	go func() {
		defer wg.Done()
		_stdout, err := io.ReadAll(stdoutPipe)
		stdout = _stdout
		if err != nil {
			HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to get stdout pipe: %w", err))
			return
		}
	}()

	err = cmd.Wait()
	if err != nil {
		HttpResponseWithError(w, http.StatusInternalServerError, fmt.Errorf("failed to wait for subprocess: %w", err))
		return
	}

	HttpResponse(w, http.StatusOK, stdout)
}

func readinessProbeHandler(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
}

var onlyOneSignalHandler = make(chan struct{})

func SetupSignalHandlerWithContext() context.Context {
	var shutdownSignals = []os.Signal{os.Interrupt, syscall.SIGTERM}

	close(onlyOneSignalHandler) // panics when called twice

	ctx, cancel := context.WithCancel(context.Background())
	c := make(chan os.Signal, 2)
	signal.Notify(c, shutdownSignals...)
	go func() {
		signal := <-c
		log.Printf("Received signal %s, exiting\n", signal.String())
		cancel()
		<-c
		panic("multiple signals received")
	}()

	return ctx
}

func main() {
	codePath := flag.String("c", DEFAULT_CODE_PATH, "Path to expected fetched executable.")
	internalCodePath := flag.String("i", DEFAULT_INTERNAL_CODE_PATH, "Path to specialized executable.")
	flag.Parse()
	absInternalCodePath, err := filepath.Abs(*internalCodePath)
	if err != nil {
		log.Fatal(err)
	}
	server := &BinaryServer{*codePath, absInternalCodePath}
	log.Printf("BinaryServer: %#v\n", server)

	mux := http.NewServeMux()
	mux.HandleFunc("/", server.InvocationHandler)
	mux.HandleFunc("/specialize", server.SpecializeHandler)
	mux.HandleFunc("/v2/specialize", server.SpecializeHandler)
	mux.HandleFunc("/healthz", readinessProbeHandler)

	httpServer := &http.Server{
		Addr:    ":8888",
		Handler: mux,
	}

	ctx := SetupSignalHandlerWithContext()
	go func() {
		err = httpServer.ListenAndServe()
		if err != nil {
			log.Fatal("ListenAndServe: ", err)
		}
	}()
	log.Println("Server started")
	<-ctx.Done()
	err = httpServer.Shutdown(ctx)
	if err != nil {
		log.Println("Server Shutdown: ", err)
	}
	os.Exit(0)
}
