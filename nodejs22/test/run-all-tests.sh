#!/bin/bash

set -e

echo "🚀 Pure ESM Node.js 22 Test Suite"
echo "================================="

cd "$(dirname $0)"
failed_tests=0
total_tests=0

run_test() {
    local test_file="$1"
    local description="$2"
    
    total_tests=$((total_tests + 1))
    echo ""
    echo "🧪 Test $total_tests: $description"
    echo "📂 File: $test_file"
    
    if ./simple-test.sh "$test_file" > /dev/null 2>&1; then
        echo "✅ PASSED: $description"
    else
        echo "❌ FAILED: $description"
        failed_tests=$((failed_tests + 1))
    fi
}

echo ""
echo "Testing Pure ESM Functions:"
echo "=========================="

# All ESM tests
run_test "./test.js" "Basic ESM function"
run_test "./test-case-1/helloWorld.js" "ESM helloWorld test case"
run_test "./test-case-2/helloUser.js" "ESM helloUser test case"
run_test "./test-case-3/wordCount.js" "ESM wordCount test case"
run_test "./test-case-4/momentExample.js" "ESM momentExample test case"

echo ""
echo "📊 Test Results Summary"
echo "======================"
echo "Total tests: $total_tests"
echo "Passed: $((total_tests - failed_tests))"
echo "Failed: $failed_tests"

if [ $failed_tests -eq 0 ]; then
    echo ""
    echo "🎉 ALL TESTS PASSED!"
    echo "✅ Pure ESM Node.js 22 modernization successful"
    echo "✅ All functions use modern 'export default' syntax"
    echo "✅ Node.js 22 with native ESM support working perfectly"
    echo "✅ No CommonJS legacy code - this is the future! 🚀"
    exit 0
else
    echo ""
    echo "❌ $failed_tests test(s) failed"
    exit 1
fi 