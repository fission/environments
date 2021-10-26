#!/bin/sh

# TODO placeholder until we have better tests :)

set +x
set -e

DIR=$(dirname $0)

# We have to remove symlink for node_modules in tests directory
if [ -h "$DIR/node_modules" ]; then
  rm "$DIR/node_modules"
fi

echo "-- Starting server"
node $DIR/../server.js --codepath $DIR/test.js --port 8888 &
cleanup() {
    echo "-- Cleanup"
    kill %1
}
trap cleanup EXIT
sleep 2

echo "-- Specializing"
curl -f -X POST http://localhost:8888/specialize

echo "-- Running user function"
curl -f -X GET http://localhost:8888
curl -f -X POST http://localhost:8888
curl -f -X PUT http://localhost:8888
curl -f -X DELETE http://localhost:8888
curl -f -X TRACE http://localhost:8888
curl -f -X OPTIONS http://localhost:8888

# -I causes curl to make a HEAD request.
curl -f -I http://localhost:8888
