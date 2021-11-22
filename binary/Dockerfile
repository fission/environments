FROM golang:1.16-alpine

WORKDIR /binary
COPY *.go /binary/

RUN go build -o server .

FROM alpine:3.14

WORKDIR /app

RUN apk update
RUN apk add coreutils binutils findutils grep

COPY --from=0 /go/server /app/server

EXPOSE 8888
ENTRYPOINT ["./server"]
