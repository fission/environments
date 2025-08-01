#!/bin/bash

set -e

ROOT_DIR="$(dirname $0)/.."
echo "🧪 Pure ESM Node.js 22 Test"

# Clean up any existing processes
pkill -f "node server.js" 2>/dev/null || true
sleep 1

test_file="$1"
if [ -z "$test_file" ]; then
    echo "Usage: $0 <test-file>"
    echo "Example: $0 ./test.js"
    exit 1
fi

# Convert to absolute path
test_file_abs="$(realpath "$test_file")"

PORT=9998

echo "📂 Testing file: $test_file"
echo "🚀 Starting server on port $PORT..."

# Start server from the correct directory
cd "$ROOT_DIR"
npm start -- --codepath "$test_file_abs" --port $PORT &
SERVER_PID=$!

# Cleanup function
cleanup() {
    echo "🧹 Cleanup: killing process $SERVER_PID"
    kill $SERVER_PID 2>/dev/null || true
    wait $SERVER_PID 2>/dev/null || true
}
trap cleanup EXIT

# Wait for server to start
sleep 3

echo "🔧 Specializing..."
if curl -f -s -X POST "http://localhost:$PORT/specialize"; then
    echo "✅ Specialization successful"
    
    echo "📨 Testing function call..."
    response=$(curl -f -s "http://localhost:$PORT/")
    echo "📤 Response: $response"
    echo "🎉 Test PASSED!"
else
    echo "❌ Specialization failed"
    exit 1
fi 