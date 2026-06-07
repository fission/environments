# Runtime architecture

## The environment contract

Every environment is an HTTP server listening on **port 8888** that fission's fetcher/executor drives:

- `POST /specialize` (v1): load user code from the fixed path `/userfunc/user`.
- `POST /v2/specialize`: JSON body `{"filepath": "...", "functionName": "..."}`; `filepath` may be a single file or a directory (built package).
- All subsequent requests on `/` (any method) are routed to the loaded user function.
- Most servers also expose `GET /healthz`.

A container specializes exactly once; pool manager replaces pods rather than re-specializing.
Unspecialized containers return an error on `/` ("Container not specialized" or similar) — that response is the expected pre-specialization behaviour, not a bug.

## functionName semantics differ per language

- **jvm**: fully-qualified class name implementing `io.fission.Function` (e.g. `io.fission.HelloWorld`).
- **ruby**: the *method* name defined by the loaded file(s), e.g. `handler` — NOT the filename.
  Passing a filename makes `method(func)` raise and specialize returns 500.
- **php**: `module::function` (e.g. `hellopsr.php::handler`).
  Without the `::` divider the env enters legacy echo mode: the file is `require`d and its buffered output is returned as the response body.
- **python**: `module.function` style handled by the server's module loader.
- **go**: entrypoint symbol in a Go plugin (`.so`) — see toolchain note below.

## Builder contract

Builder images run `/usr/local/bin/build` (from `build.sh`/`defaultBuildCmd`) with `SRC_PKG` and `DEPLOY_PKG` env vars, transforming a source package into a deploy package.
Examples: maven `package` (jvm), `bundle install` with deployment config (ruby), `composer install` (php), `pip install -r requirements.txt -t` (python), go plugin build.

## Language-specific runtime notes

- **go**: functions are Go plugins; the function build toolchain MUST exactly match the env server's toolchain.
  Env and builder Dockerfiles share the `GO_VERSION` build arg — always bump them together.
- **jvm**: depends on `io.fission:fission-java-core`, which is not resolvable from any remote repository (see environment-notes.md); it is built from source by `install-fission-java-core.sh` in the env image, the builder image (pre-seeded `/root/.m2`), and the CI test container.
- **ruby**: `fission/specializer.rb` loads vendored gems from `vendor/bundle/ruby/*/gems/*/lib` and native extensions via a platform-wildcard glob (images are musl, amd64+arm64 — never hardcode a platform dir).
  Stay on Rack 2.2.x: `server.rb` uses `Rack::Handler::Thin`, removed in Rack 3.
- **php**: react/http 1.x `HttpServer` with the auto-run global loop; uncaught handler Throwables become 500s and the process keeps serving (an `error` listener logs them).
  `ob_start` must be balanced on every early-return path — the process is long-running, leaked buffer levels accumulate and can corrupt later echo-mode responses.
- **python**: serves via bjoern by default or gevent (`WSGI_FRAMEWORK=GEVENT`), with vendored `flask_sockets.py` for websockets (Werkzeug 3 moved `parse_cookie` to `werkzeug.sansio.http`).
- **perl**: Dancer2 + Twiggy; only `/specialize` (v1) and `/` routes.
- **tensorflow-serving**: a Go proxy (`server.go`) in front of `tensorflow_model_server`; built with go modules initialized at image build time.
