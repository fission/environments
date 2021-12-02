#!/bin/bash

# TODO placeholder until we have better tests :)
TMPPATH="/tmp/binary"
SERVER="http://localhost:8888"
set -x
set -e

DIR=$(dirname $0)
if [ -d "$TMPPATH" ] ; then
    rm -r $TMPPATH
fi

mkdir -p "$TMPPATH"/userfunc/user
mkdir -p "$TMPPATH"/bin

echo "-- Starting server"
go run server.go env.go -i "$TMPPATH"/bin/userfunc &
SERVER_PID=$!

cleanup() {
    echo "-- Cleanup"
    echo "Killing process $SERVER_PID"
    kill $SERVER_PID
}
trap cleanup EXIT
sleep 5

echo "--Healthz"
curl -i -f -X GET "$SERVER"/healthz

echo "-- Specializing"
curl -i -f -XPOST "$SERVER"/v2/specialize -H 'Content-Type: application/json' -d '{"filepath": "./examples/echo.sh"}'

echo "-- Running"
curl -i -f -XPOST "$SERVER" -d 'Echoooooo!'

echo "-- Done running jobs"

echo "-- Background jobs"
jobs