FROM golang:1.16-alpine

WORKDIR /binary
COPY *.go /binary/

RUN go mod init github.com/fission/environments/binary
RUN go mod tidy

RUN go build -o server .

FROM alpine:3.15

WORKDIR /app

RUN apk update
RUN apk add --no-cache coreutils binutils findutils grep

COPY --from=0 /binary/server /app/server

EXPOSE 8888
ENTRYPOINT ["./server"]
