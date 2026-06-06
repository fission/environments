ARG GO_VERSION=1.26

FROM tensorflow/serving:2.20.0 AS serving
RUN apt update && apt install -y ca-certificates && rm -rf /var/lib/apt/lists/*

FROM golang:${GO_VERSION} AS builder

ENV APP /src/tensorflow-serving

WORKDIR ${APP}

ADD server.go ${APP}

RUN go mod init github.com/fission/environments/tensorflow-serving && \
    go mod tidy && \
    go build -a -o /server server.go

FROM serving
WORKDIR /
COPY --from=builder /server /

ENTRYPOINT ["/server"]
EXPOSE 8888
