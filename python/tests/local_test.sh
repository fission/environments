#!/bin/bash

# TODO placeholder until we have better tests :)

set -x
set -e

DIR=$(dirname $0)

python3 -m virtualenv test_env
source test_env/bin/activate

pip3 install -r requirements.txt

echo "-- Starting server"
python3 $DIR/../server.py &

SERVER_PID=$!
cleanup() {
    echo "-- Cleanup"
    echo "Killing process $SERVER_PID"
    pkill -f '.*server.py'
    kill $SERVER_PID
    deactivate
    rm -r test_env
    ps -ef | grep python3 | grep -v grep
}
trap cleanup EXIT
sleep 5

echo "--Healthz"
curl -f -X GET http://localhost:$RUNTIME_PORT/healthz

echo "-- Specializing"
curl -f -XPOST http://localhost:$RUNTIME_PORT/v2/specialize -H 'Content-Type: application/json' -d '{"filepath": "./examples/hello.py", "functionName": "main"}'

echo "-- Running user function"
echo "-- GET request"
curl -f -X GET http://localhost:$RUNTIME_PORT
echo "-- POST request"
curl -f -X POST http://localhost:$RUNTIME_PORT
echo "-- PUT request"
curl -f -X PUT http://localhost:$RUNTIME_PORT
echo "-- DELETE request"
curl -f -X DELETE http://localhost:$RUNTIME_PORT
echo "-- OPTIONS request"
curl -f -X OPTIONS http://localhost:$RUNTIME_PORT
echo "-- HEAD request"
# -I causes curl to make a HEAD request.
curl -f -I http://localhost:$RUNTIME_PORT
echo "-- Done running jobs"

echo "-- Background jobs"
jobs