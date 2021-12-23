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

cleanup() {
    echo "-- Cleanup"
    echo "Killing process $SERVER_PID"
    pkill -f '.*server.py'
    deactivate
    rm -r test_env
}
trap cleanup EXIT
sleep 5

echo "--Healthz"
curl -f -X GET http://localhost:8888/healthz

echo "-- Specializing"
curl -f -XPOST http://localhost:8888/v2/specialize -H 'Content-Type: application/json' -d '{"filepath": "./examples/hello.py", "functionName": "main"}'

echo "-- Running user function"
echo "-- GET request"
curl -f -X GET http://localhost:8888
echo "-- POST request"
curl -f -X POST http://localhost:8888
echo "-- PUT request"
curl -f -X PUT http://localhost:8888
echo "-- DELETE request"
curl -f -X DELETE http://localhost:8888
echo "-- OPTIONS request"
curl -f -X OPTIONS http://localhost:8888
echo "-- HEAD request"
# -I causes curl to make a HEAD request.
curl -f -I http://localhost:8888
echo "-- Done running jobs"

echo "-- Background jobs"
jobs