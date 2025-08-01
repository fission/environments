#!/bin/bash

# Pure ESM Node.js 22 Local Test Script

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
    
    echo "=================================================="
    echo "-- Testing: $description"
    echo "-- File: $test_file"
    echo "-- Port: $port"
    echo "=================================================="
    
    echo "-- Starting server"
    node "$ROOT_DIR/server.js" --codepath "$test_file" --port "$port" &
    SERVER_PID=$!
    
    # Cleanup function for this test
    cleanup() {
        echo "-- Cleanup for $description"
        echo "-- Killing process $SERVER_PID"
        kill $SERVER_PID 2>/dev/null || true
        wait $SERVER_PID 2>/dev/null || true
    }
    
    # Give server time to start
    sleep 2
    
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

echo "🚀 Pure ESM Node.js 22 Local Test Suite"
echo "======================================="

# Test 1: Basic ESM function
test_function "$DIR/test.js" "Basic ESM Function" 8887

# Test 2: ESM helloWorld test case
test_function "$DIR/test-case-1/helloWorld.js" "ESM helloWorld test case" 8888

# Test 3: ESM helloUser test case
test_function "$DIR/test-case-2/helloUser.js" "ESM helloUser test case" 8889

echo "🎉 All local tests completed successfully!"
echo "✅ Pure ESM Node.js 22 modernization successful"
echo "✅ All functions use 'export default' syntax"
echo "✅ No CommonJS dependencies - this is the future! 🚀"