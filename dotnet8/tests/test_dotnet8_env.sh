#!/bin/bash

set -euo pipefail
ROOT=$(dirname $0)/../..
source $ROOT/test_utils/utils.sh

TEST_ID=$(generate_test_id)
echo "TEST_ID = $TEST_ID"

tmp_dir="/tmp/test-$TEST_ID"
mkdir -p $tmp_dir

cleanup() {
    log "Cleaning up..."
    clean_resource_by_id $TEST_ID
    rm -rf $tmp_dir
}

if [ -z "${TEST_NOCLEANUP:-}" ]; then
    trap cleanup EXIT
else
    log "TEST_NOCLEANUP is set; not cleaning up test artifacts afterwards."
fi

env=dotnet8-$TEST_ID
fn_poolmgr=hello-dotnet8-pm-$TEST_ID
fn_nd=hello-dotnet8-nd-$TEST_ID

cd $ROOT/dotnet8/examples

DOTNET8_BUILDER_IMAGE=${DOTNET8_BUILDER_IMAGE:-davidchase03/dotnet8-builder:v21.1}
DOTNET8_RUNTIME_IMAGE=${DOTNET8_RUNTIME_IMAGE:-davidchase03/dotnet8-env:v21.0}

log "Creating environment for .NET 8"
log "DOTNET8_RUNTIME_IMAGE = $DOTNET8_RUNTIME_IMAGE"
log "DOTNET8_BUILDER_IMAGE = $DOTNET8_BUILDER_IMAGE"
fission env create --name $env --image $DOTNET8_RUNTIME_IMAGE --builder $DOTNET8_BUILDER_IMAGE --poolsize 1

# Wait for builder to be ready
log "Waiting for builder to be ready (this may take a few minutes)..."
timeout 300s bash -c "wait_for_builder $env" || {
    log "Warning: Builder may not be fully ready, continuing anyway..."
}

# Test 1: Simple HelloWorld with builder
log "===== Test 1: HelloWorld with builder ====="
pushd HelloWorld > /dev/null
zip -qr $tmp_dir/hello.zip MyFunction.cs HelloWorld.csproj -x "bin/*" -x "obj/*" -x "*.dll"
popd > /dev/null

pkgName=$(generate_test_id)
fission package create --name $pkgName --src $tmp_dir/hello.zip --env $env

log "Waiting for build to complete..."
timeout 120s bash -c "waitBuild $pkgName"

log "Creating pool manager & new deployment functions"
fission fn create --name $fn_poolmgr --env $env --pkg $pkgName --executortype poolmgr
fission fn create --name $fn_nd --env $env --pkg $pkgName --executortype newdeploy --minscale 0 --maxscale 1

log "Creating routes"
fission route create --name $fn_poolmgr --function $fn_poolmgr --url /$fn_poolmgr --method GET
fission route create --name $fn_nd --function $fn_nd --url /$fn_nd --method GET

log "Waiting for router to catch up..."
sleep 5

log "Testing pool manager function"
timeout 60 bash -c "test_fn $fn_poolmgr 'Hello World'"

log "Testing new deployment function"
timeout 60 bash -c "test_fn $fn_nd 'Hello World'"

# Test 2: Multi-file project
log "===== Test 2: Multi-file project with builder ====="
pushd MultiFileExample > /dev/null
zip -qr $tmp_dir/multifile.zip * -x "*.zip" -x "bin/*" -x "obj/*" -x "*.dll"
popd > /dev/null

pkgName=$(generate_test_id)
fission package create --name $pkgName --src $tmp_dir/multifile.zip --env $env

log "Waiting for build to complete..."
timeout 120s bash -c "waitBuild $pkgName"

log "Updating functions with new package"
fission fn update --name $fn_poolmgr --pkg $pkgName
fission fn update --name $fn_nd --pkg $pkgName

log "Waiting for updates to propagate..."
sleep 5

log "Testing pool manager function with multifile package"
timeout 60 bash -c "test_fn $fn_poolmgr 'Multi-File Example API'"

log "Testing new deployment function with multifile package"
timeout 60 bash -c "test_fn $fn_nd 'Multi-File Example API'"

log "Test PASSED"