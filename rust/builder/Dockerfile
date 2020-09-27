ARG BUILDER_IMAGE=fission/builder:latest
ARG RUST_VERSION=1-alpine

FROM ${BUILDER_IMAGE}

FROM rust:${RUST_VERSION}

ENV APP /app
WORKDIR ${APP}

COPY --from=0 /builder /builder
ADD build.sh /usr/local/bin/build
