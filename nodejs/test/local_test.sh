#!/bin/bash

# Node.js test script with ESM/CJS support via LOAD_ESM environment variable

set +x
set -e

DIR=$(dirname $0)
ROOT_DIR="$DIR/.."

# We have to remove symlink for node_modules in tests directory
if [ -h "$DIR/node_modules" ]; then
  rm "$DIR/node_modules"
fi

# Function to test a specific function file
test_function() {
    local test_file=$1
    local description=$2
    local port=$3
    local load_esm=${4:-"false"}
    
    echo "=================================================="
    echo "-- Testing: $description"
    echo "-- File: $test_file"
    echo "-- Port: $port"
    echo "-- LOAD_ESM: $load_esm"
    echo "=================================================="
    
    # Check if port is in use and kill any existing process
    local existing_pid=$(lsof -ti:$port)
    if [ ! -z "$existing_pid" ]; then
        echo "-- Killing existing process on port $port: $existing_pid"
        kill -9 $existing_pid 2>/dev/null || true
        sleep 1
    fi
    
    echo "-- Starting server"
    LOAD_ESM="$load_esm" node "$ROOT_DIR/server.js" --codepath "$test_file" --port "$port" &
    SERVER_PID=$!
    
    # Cleanup function for this test
    cleanup() {
        echo "-- Cleanup for $description"
        echo "-- Killing process $SERVER_PID"
        kill -9 $SERVER_PID 2>/dev/null || true
        wait $SERVER_PID 2>/dev/null || true
        sleep 1
    }
    
    # Give server time to start
    sleep 3
    
    echo "-- Specializing"
    if curl -f -X POST "http://localhost:$port/specialize"; then
        echo ""
        echo "-- Specialization successful"
        
        echo "-- Running user function tests"
        echo "-- GET request"
        curl -f -X GET "http://localhost:$port"
        echo ""
        echo "-- POST request"
        curl -f -X POST "http://localhost:$port"
        echo ""
        echo "-- PUT request"
        curl -f -X PUT "http://localhost:$port"
        echo ""
        echo "-- DELETE request"
        curl -f -X DELETE "http://localhost:$port"
        echo ""
        echo "-- OPTIONS request"
        curl -f -X OPTIONS "http://localhost:$port"
        echo ""
        echo "-- HEAD request"
        curl -f -I "http://localhost:$port"
        echo ""
        echo "-- ✅ All tests passed for $description"
    else
        echo ""
        echo "-- ❌ Specialization failed for $description"
        cleanup
        return 1
    fi
    
    cleanup
    echo "-- Done with $description"
    echo ""
}

echo "🚀 Node.js Local Test Suite with ESM/CJS Support"
echo "==============================================="

# Install dependencies for test cases that need them
echo "-- Installing dependencies for test cases with package.json..."
if [ -f "$DIR/test-case-4/package.json" ]; then
    echo "-- Installing dependencies for test-case-4..."
    (cd "$DIR/test-case-4" && npm install --silent)
fi
if [ -f "$DIR/test-case-8/package.json" ]; then
    echo "-- Installing dependencies for test-case-8..."
    (cd "$DIR/test-case-8" && npm install --silent)
fi

# Clean up any existing processes on our test ports
echo "-- Cleaning up any existing processes on test ports..."
for port in 8881 8882 8883 8884 8885 8886 8887 8888 8889; do
    existing_pid=$(lsof -ti:$port 2>/dev/null || true)
    if [ ! -z "$existing_pid" ]; then
        echo "-- Killing existing process on port $port: $existing_pid"
        kill -9 $existing_pid 2>/dev/null || true
    fi
done
sleep 2

# Test CJS functions (LOAD_ESM=false)
echo "Testing CJS test cases..."
test_function "$DIR/test-case-1/helloWorld.js" "CJS helloWorld test case" 8881 "false"
test_function "$DIR/test-case-2/helloUser.js" "CJS helloUser test case" 8882 "false"
test_function "$DIR/test-case-3/wordCount.js" "CJS wordCount test case" 8883 "false"
test_function "$DIR/test-case-4/momentExample.js" "CJS momentExample with dependencies" 8884 "false"

# Test ESM functions (LOAD_ESM=true)  
echo "Testing ESM test cases..."
test_function "$DIR/test-case-5/helloWorld.js" "ESM helloWorld test case" 8885 "true"
test_function "$DIR/test-case-6/helloUser.js" "ESM helloUser test case" 8886 "true"
test_function "$DIR/test-case-7/wordCount.js" "ESM wordCount test case" 8887 "true"
test_function "$DIR/test-case-8/momentExample.js" "ESM momentExample with dependencies" 8888 "true"

# Test basic test.js (CJS)
test_function "$DIR/test.js" "Basic CJS Function" 8889 "false"

echo "🎉 All local tests completed successfully!"
echo "✅ Both ESM and CJS functions tested"
echo "✅ LOAD_ESM environment variable working correctly"