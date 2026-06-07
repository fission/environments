#!/bin/bash
# Cluster-free test: exercises the supervisor's env contract (healthz,
# v2 specialize, proxying) against locally compiled function binaries.
set -ex

DIR=$(realpath "$(dirname "$0")")/..
TMPPATH=$(mktemp -d)
SERVER="http://localhost:8888"

kill_processes() {
    # Free the supervisor and function ports, whatever holds them.
    for port in 8888 8889; do
        lsof -ti tcp:$port | xargs kill 2>/dev/null || true
    done
}

cleanup() {
    kill_processes
    rm -rf "$TMPPATH"
}
trap cleanup EXIT
kill_processes

cd "$DIR"
cargo build --release -p supervisor -p fission-function
(cd examples/project-example && cargo build --release)

wait_for_server() {
    for _ in $(seq 1 50); do
        curl -s -f -o /dev/null "$SERVER/healthz" && return 0
        sleep 0.2
    done
    echo "supervisor did not become ready"
    return 1
}

echo "-- Phase 1: project-mode binary (echo)"
mkdir -p "$TMPPATH/deploy-echo"
cp examples/project-example/target/release/echo-example "$TMPPATH/deploy-echo/handler"

./target/release/supervisor &
wait_for_server

echo "-- Requests before specialize must fail"
status=$(curl -s -o /dev/null -w '%{http_code}' -XPOST "$SERVER")
[ "$status" = "500" ]

echo "-- Specialize (directory deploy package)"
curl -i -f -XPOST "$SERVER/v2/specialize" -H 'Content-Type: application/json' \
    -d "{\"filepath\": \"$TMPPATH/deploy-echo\", \"functionName\": \"\"}"

echo "-- Invoke: JSON body is echoed back"
curl -s -f -XPOST "$SERVER" -H 'Content-Type: application/json' -d '{"name": "fission"}' |
    grep '"echo":{"name":"fission"}'

kill_processes
sleep 1

echo "-- Phase 2: template (single-file mode) binary"
./target/release/supervisor &
wait_for_server

echo "-- Specialize (file path + entrypoint resolution)"
curl -i -f -XPOST "$SERVER/v2/specialize" -H 'Content-Type: application/json' \
    -d "{\"filepath\": \"$DIR/target/release/handler\"}"

curl -s -f "$SERVER" | grep "hello from the fission rust template"

echo "-- All local tests passed"
