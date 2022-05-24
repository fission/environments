#!/bin/bash

# TODO placeholder until we have better tests :)

set -x
set -e

DIR=$(dirname $0)
export RUNTIME_PORT=8083
export MOCK_FETCHER=true
export WSGI_FRAMEWORK=GEVENT
export USERFUNCVOL=/tmp
export TIMEOUT=5

if ! [ -d test_env ]; then
    python3 -m virtualenv test_env
fi
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
    # deactivate
    # rm -r test_env
    ps -ef | grep python3 | grep -v grep
}
trap cleanup EXIT
sleep 5

echo "--Healthz"
curl -f -X GET http://localhost:$RUNTIME_PORT/healthz

echo "-- Specializing"
curl -f -XPOST http://localhost:$RUNTIME_PORT/v2/specialize -H 'Content-Type: application/json' -d '{"filepath": "./examples/websocket/main.py", "functionName": "main"}'

echo "-- Websocket ready to connect ws://localhost:$RUNTIME_PORT/."
read -p "Press enter to continue"

echo "-- Running user function"

echo "-- Done running jobs"

echo "-- Background jobs"
jobs