# Fission: Rust Environment

This is the Rust environment for [Fission](https://fission.io).

Unlike interpreted-language environments, Rust functions are compiled by the builder into a complete native HTTP server binary (built on [axum](https://docs.rs/axum) and [tokio](https://tokio.rs)).
The runtime image runs a small supervisor that implements the Fission environment contract on port 8888, spawns the function binary exactly once at specialization, and reverse-proxies all requests to it over a pooled keep-alive connection.
There is no per-request process spawn and no dynamic library loading; steady-state overhead is a single localhost proxy hop.

```
┌─ rust-env pod ───────────────────────────────┐
│  supervisor :8888                            │
│   ├ GET  /healthz                            │
│   ├ POST /specialize, /v2/specialize         │
│   └ /*  ── streaming reverse proxy ──▶ child │
│  function binary 127.0.0.1:8889              │
│  (spawned once at specialization)            │
└──────────────────────────────────────────────┘
```

## Writing functions

### Single-file mode

Provide one `.rs` file defining `pub async fn handler` with any [axum handler](https://docs.rs/axum/latest/axum/handler/index.html) signature:

```rust
use fission_rust::IntoResponse;

pub async fn handler() -> impl IntoResponse {
    "Hello, World!\n"
}
```

```sh
fission package create --name hello --src hello.rs --env rust
```

Single-file functions may use the crates pre-baked into the builder's template: `fission-rust`, `axum`, `tokio`, `serde`, and `serde_json`.
A source archive may also contain a `handler.rs` plus extra module files; each file becomes a crate-root module, so `handler.rs` can reach a sibling `util.rs` as `crate::util`.
For anything more, use a Cargo project.

### Cargo project mode

Provide a source archive containing a Cargo binary crate (a `Cargo.toml` at the archive root).
The only contract: the binary must serve HTTP on `127.0.0.1:$FISSION_RUNTIME_PORT`.
Any framework works — axum, actix-web, rocket, warp — see [rust/project-example in fission/examples](https://github.com/fission/examples/tree/main/rust/project-example).

```sh
# from a clone of https://github.com/fission/examples
cd rust/project-example && zip -r /tmp/project.zip Cargo.toml src
fission package create --name echo --src /tmp/project.zip --env rust
```

If the project defines multiple `[[bin]]` targets, all are deployed and the function's `--entrypoint` selects the binary by name.
A single binary is always deployed as `handler` and needs no entrypoint.

To use the `fission-rust` SDK in project mode, reference it as a git dependency (works both on your machine and inside the builder):

```toml
[dependencies]
fission-rust = { git = "https://github.com/fission/environments" }
```

For air-gapped builds, the builder image also vendors the crate at `/usr/src/fission/fission-rust`, usable as a `path` dependency inside the builder.

## Layout

- `supervisor/` — the environment server (specialize, readiness, proxy).
- `fission-rust/` — the SDK crate (`fission_rust::serve(handler)`).
- `template/` — the crate the builder wraps around single-file functions.
- `builder/build.sh` — detects project vs single-file mode and compiles the deploy package.

## Build it yourself

```sh
# runtime + builder images (local, single arch)
cd rust/ && make DOCKER_FLAGS=--load PLATFORMS=linux/arm64
```

## Tests

```sh
cd rust/ && cargo test          # unit tests
./test/local_test.sh            # cluster-free contract test
./tests/test_rust_env.sh        # e2e against a kind cluster
```

## Notes

- Builder and runtime images must stay on the same Debian release (`bookworm`) so the glibc versions match; bump `RUST_VERSION`/`DEBIAN_VERSION` together in `Makefile`, `builder/Makefile`, and `skaffold.yaml`.
- The function process inherits stdout/stderr, so its logs appear in `fission fn log`.
- If the function process exits, the supervisor exits too and Fission replaces the pod.
- The Rust logo is from [rust-lang/rust-artwork](https://github.com/rust-lang/rust-artwork) (CC-BY).
