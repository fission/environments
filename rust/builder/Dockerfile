# Build context is the parent directory (rust/), see builder/Makefile.
ARG BUILDER_IMAGE=ghcr.io/fission/builder:v1.25.0
ARG RUST_VERSION=1.96
ARG DEBIAN_VERSION=bookworm

FROM ${BUILDER_IMAGE}
FROM rust:${RUST_VERSION}-${DEBIAN_VERSION}

COPY --from=0 /builder /builder

RUN apt-get update \
    && apt-get install -y --no-install-recommends jq \
    && rm -rf /var/lib/apt/lists/*

# Vendor the workspace (SDK + template) for single-file function builds.
WORKDIR /usr/src/fission
COPY Cargo.toml Cargo.lock rust-toolchain.toml ./
COPY fission-rust/ fission-rust/
COPY template/ template/
COPY supervisor/ supervisor/

# Shared target dir, pre-warmed below: single-file builds only recompile
# the handler, and project builds reuse compiled registry crates.
ENV CARGO_TARGET_DIR=/usr/src/fission/target \
    CARGO_INCREMENTAL=0 \
    CARGO_TERM_COLOR=never
RUN cargo build --release -p fission-function

COPY builder/build.sh /usr/local/bin/build
RUN chmod +x /usr/local/bin/build

EXPOSE 8001
