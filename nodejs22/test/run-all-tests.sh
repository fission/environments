#!/bin/bash

set -e

echo "🚀 Node.js 22 Test Suite (ESM + CJS)"
echo "====================================="

cd "$(dirname $0)"
failed_tests=0
total_tests=0

run_test() {
    local test_file="$1"
    local description="$2"
    local load_esm="${3:-true}"  # Default to ESM mode
    
    total_tests=$((total_tests + 1))
    echo ""
    echo "🧪 Test $total_tests: $description"
    echo "📂 File: $test_file"
    
    # Set LOAD_ESM environment variable for the test
    if LOAD_ESM="$load_esm" ./simple-test.sh "$test_file" > /dev/null 2>&1; then
        echo "✅ PASSED: $description"
    else
        echo "❌ FAILED: $description"
        failed_tests=$((failed_tests + 1))
    fi
}

echo ""
echo "Testing ESM Functions:"
echo "====================="

# All ESM tests
run_test "./test.js" "Basic ESM function"
run_test "./test-case-1/helloWorld.js" "ESM helloWorld test case"
run_test "./test-case-2/helloUser.js" "ESM helloUser test case"
run_test "./test-case-3/wordCount.js" "ESM wordCount test case"
run_test "./test-case-4/momentExample.js" "ESM momentExample test case"

echo ""
echo "Testing CJS Functions (.js extension with LOAD_ESM=false):"
echo "========================================================="
echo ""

# CJS tests with .js extension (using LOAD_ESM=false environment)
run_test "./test-case-cjs-1/hello.js" "CJS basic function test case (.js)" "false"
run_test "./test-case-cjs-2/handler.js" "CJS callback pattern test case (.js)" "false" 
run_test "./test-case-cjs-3/multiExport.js" "CJS multiple exports test case (.js)" "false"

echo ""
echo "📊 Test Results Summary"
echo "======================"
echo "Total tests: $total_tests"
echo "Passed: $((total_tests - failed_tests))"
echo "Failed: $failed_tests"

if [ $failed_tests -eq 0 ]; then
    echo ""
    echo "🎉 ALL TESTS PASSED!"
    echo "✅ Node.js 22 LOAD_ESM dual module support successful"
    echo "✅ ESM functions using modern 'export default' syntax with .js extension"
    echo "✅ CJS functions using traditional 'module.exports' syntax with .js extension"
    echo "✅ LOAD_ESM environment variable controls .js file interpretation"
    echo "✅ Single image, dual functionality! 🚀"
    exit 0
else
    echo ""
    echo "❌ $failed_tests test(s) failed"
    exit 1
fi 