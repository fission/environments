ARG BUILDER_IMAGE=fission/builder:latest
ARG GO_VERSION=1.12

FROM ${BUILDER_IMAGE}

FROM golang:${GO_VERSION}

ENV GOPATH /usr
WORKDIR ${GOPATH}

COPY --from=0 /builder /builder
ADD build.sh /usr/local/bin/build

