ARG RUST_VERSION=1.96
ARG DEBIAN_VERSION=bookworm

FROM rust:${RUST_VERSION}-${DEBIAN_VERSION} AS builder

WORKDIR /src
COPY Cargo.toml Cargo.lock rust-toolchain.toml ./
COPY supervisor/ supervisor/
COPY fission-rust/ fission-rust/
COPY template/ template/

RUN cargo build --release -p supervisor

# Runtime base must share the builder's libc (both ${DEBIAN_VERSION});
# bump the two stages together.
FROM debian:${DEBIAN_VERSION}-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY --from=builder /src/target/release/supervisor /usr/local/bin/supervisor

ENTRYPOINT ["/usr/local/bin/supervisor"]
EXPOSE 8888
