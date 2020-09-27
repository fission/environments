ARG GO_VERSION=1.15-alpine3.12
ARG ALPINE_VERSION=3.12
FROM golang:${GO_VERSION}

WORKDIR /go
COPY *.go /go/

RUN GOOS=linux GOARCH=386 go build -o server .

FROM alpine:${ALPINE_VERSION}

WORKDIR /app

RUN apk update
RUN apk add coreutils binutils findutils grep

COPY --from=0 /go/server /app/server

EXPOSE 8888
ENTRYPOINT ["./server"]
