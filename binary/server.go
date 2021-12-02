package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"os/exec"
	"path/filepath"
	"strings"
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
	execEnv.SetEnv(&EnvVar{"PATH", "$PATH:/userfunc/deployarchive:/userfunc"})

	for header, val := range r.Header {
		execEnv.SetEnv(&EnvVar{fmt.Sprintf("HTTP_%s", strings.ToUpper(header)), val[0]})
	}

	// Future: could be improved by keeping subprocess open while environment is specialized
	cmd := exec.Command("/bin/sh", bs.internalCodePath)
	cmd.Env = execEnv.ToStringEnv()

	if r.ContentLength != 0 {
		log.Println(r.ContentLength)
		stdin, err := cmd.StdinPipe()
		if err != nil {
			HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to get stdin pipe: %w", err))
			return
		}
		written, err := io.Copy(stdin, r.Body)
		if err != nil {
			HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("failed to copy body to stdin: %w", err))
			return
		}
		log.Printf("Wrote %d bytes to stdin\n", written)
	}

	out, err := cmd.Output()
	if err != nil {
		HttpResponseWithError(w, http.StatusBadRequest, fmt.Errorf("function error: %w\n%s", err, string(out)))
		return
	}

	HttpResponse(w, http.StatusOK, out)
}

func readinessProbeHandler(w http.ResponseWriter, r *http.Request) {
	w.WriteHeader(http.StatusOK)
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
	http.HandleFunc("/", server.InvocationHandler)
	http.HandleFunc("/specialize", server.SpecializeHandler)
	http.HandleFunc("/v2/specialize", server.SpecializeHandler)
	http.HandleFunc("/healthz", readinessProbeHandler)

	log.Println("Listening on 8888 ...")
	err = http.ListenAndServe(":8888", nil)
	if err != nil {
		log.Fatal("ListenAndServe: ", err)
	}
}
