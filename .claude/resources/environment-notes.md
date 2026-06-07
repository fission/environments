# Per-environment notes

State as of the June 2026 dependency-update series (PRs #436–#446, #450–#451).

## jvm (Spring Boot)

- Java 25 LTS (eclipse-temurin alpine), Spring Boot 3.5.x, Maven 3.9.x.
- `io.fission:fission-java-core` was only ever published as `0.0.2-SNAPSHOT` to oss.sonatype.org (OSSRH), decommissioned in 2025 — it resolves from **no** remote repository.
  `install-fission-java-core.sh` builds it from a pinned commit of [fission/fission-java-libs](https://github.com/fission/fission-java-libs) and installs it locally as `0.0.1`, `0.0.2`, and `0.0.2-SNAPSHOT` (the SNAPSHOT keeps pre-existing user functions building).
  The script exists twice (env context and `builder/` context) — keep both copies in sync.
  The library's 2018-era pom lacks XML namespace declarations; the script patches the root element before running maven plugins.
- The CI test builds the example jar in a clean maven container, so the test script must run the install script there too.

## jvm-jersey

- Jersey 2.x (javax namespace) on Jetty 9.4.x, Java 25; depends on `io.fission:fission-jvm-jersey:0.0.1` which IS on Maven Central (unlike fission-java-core).
- Image names carry the Java version suffix (`jvm-jersey-env-25`, `jvm-jersey-builder-25`); renaming requires touching Makefile target names, envconfig, and the fission.io catalog.

## python / python-fastapi

- `python:3.13-alpine`; Flask 3.x + bjoern/gevent, FastAPI + uvicorn.
- bjoern needs libev headers (alpine: in image; macOS local: `brew install libev` with `CFLAGS=-I/opt/homebrew/include LDFLAGS=-L/opt/homebrew/lib`).
- `flask_sockets.py` is vendored (upstream dead); Werkzeug ≥2.3 moved `parse_cookie` to `werkzeug.sansio.http`.
- Local tests: `USERFUNCVOL=/tmp RUNTIME_PORT=<port> ./tests/local_test.sh`, then repeat with `WSGI_FRAMEWORK=GEVENT` — the gevent path exercises the fragile websocket stack.

## nodejs

- Three image flavours from one Dockerfile via `NODE_BASE_IMG`: `node-env` + `node-env-22` (alpine) and `node-env-debian`.
- ESM-first server with CJS support; `test/local_test.sh` covers both loaders.
- The Dockerfile copies only `package.json` (not the lockfile), so images resolve dependency floors at build time — lockfile refreshes need a version bump + rebuild to reach the published image.

## go

- Versioned image pair (`go-env-1.xx`, `go-builder-1.xx`) plus unversioned aliases; bump = rename targets/images in `go/Makefile`, `go/builder/Makefile`, `envconfig.json`, `skaffold.yaml`, the example spec, and the fission.io catalog.
- Plugin model: function `.so` must be built with the exact toolchain of the env server — env and builder share `GO_VERSION`.

## binary

- Alpine + a small Go server executing arbitrary binaries; `go mod init`/`tidy` at image build (stdlib only).

## ruby

- `ruby:3.4-alpine`; Rack pinned `~> 2.2` (Rack 3 removed `Rack::Handler`, which `server.rb` uses via thin).
- Regenerate `Gemfile.lock` inside the target container and `bundle lock --add-platform` for both gnu and musl, amd64 and arm64.
- Builder uses bundler deployment config (`bundle config set --local deployment true`), not the deprecated `--deployment` flag.

## php7 (directory name kept; runs PHP 8.3)

- `php:8.3-alpine`, react/http 1.x, Monolog 3, php-parser 5.
- Only compile extensions NOT bundled with the official image; rebuilding bundled exts (e.g. iconv) fails on musl.
  `json` is core; `xmlrpc`/`mcrypt` were removed from PHP 8 and their PECL ports are unmaintained.
- The directory and image name stay `php7`/`php-env` — path filters and release derivation depend on them.

## perl

- Pinned `perl:5.42`; Dancer2 + Twiggy; v1 specialize only.

## tensorflow-serving

- Pinned `tensorflow/serving` tag; upstream publishes **amd64 only** — the Makefile target overrides `PLATFORMS=linux/amd64`.
- Go proxy built with modules initialized at build time (`pkg/errors`, `zap`).

## dotnet / dotnet20 (frozen legacy)

- .NET Core 1.1 / 2.0, both EOL years ago, on the removed `microsoft/dotnet` Docker Hub images.
- Intentionally untouched; their release matrix legs fail if a reconcile run picks them up — expected.
- `dotnet8/` is the supported .NET path.
