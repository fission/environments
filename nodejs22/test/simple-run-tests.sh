#!/bin/bash

echo "🚀 Simple Pure ESM Node.js 22 Test"
echo "=================================="

cd "$(dirname $0)"

# Clean up first
pkill -f "node.*server.js" 2>/dev/null || true
sleep 1

test_single_function() {
    local test_file="$1"
    local description="$2"
    local port="$3"
    
    echo ""
    echo "🧪 Testing: $description"
    echo "📂 File: $test_file"
    echo "🔌 Port: $port"
    
    # Start server
    node "../server.js" --codepath "$test_file" --port "$port" &
    local server_pid=$!
    
    # Wait for server to start
    sleep 3
    
    # Test specialization
    if curl -f -s -X POST "http://localhost:$port/specialize" > /dev/null 2>&1; then
        echo "✅ Specialization: SUCCESS"
        
        # Test function call
        if curl -f -s "http://localhost:$port/" > /dev/null 2>&1; then
            echo "✅ Function call: SUCCESS"
            echo "🎉 $description: PASSED"
            local result=0
        else
            echo "❌ Function call: FAILED"
            local result=1
        fi
    else
        echo "❌ Specialization: FAILED"
        local result=1
    fi
    
    # Cleanup
    kill $server_pid 2>/dev/null || true
    wait $server_pid 2>/dev/null || true
    sleep 1
    
    return $result
}

# Run tests
passed=0
total=0

echo ""
echo "Testing Pure ESM Functions:"
echo "=========================="

# Test each function
for test_case in \
    "./test.js|Basic ESM function|9990" \
    "./test-case-1/helloWorld.js|ESM helloWorld|9991" \
    "./test-case-2/helloUser.js|ESM helloUser|9992"
do
    IFS='|' read -r file desc port <<< "$test_case"
    total=$((total + 1))
    
    if test_single_function "$file" "$desc" "$port"; then
        passed=$((passed + 1))
    fi
done

echo ""
echo "📊 Results Summary"
echo "=================="
echo "Total tests: $total"
echo "Passed: $passed"
echo "Failed: $((total - passed))"

if [ $passed -eq $total ]; then
    echo ""
    echo "🎉 ALL TESTS PASSED!"
    echo "✅ Pure ESM Node.js 22 working perfectly!"
    exit 0
else
    echo ""
    echo "❌ Some tests failed"
    exit 1
fi 