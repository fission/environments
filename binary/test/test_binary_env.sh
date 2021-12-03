#!/bin/bash

set -euo pipefail
ROOT=$(dirname $0)/../..
source $ROOT/test_utils/utils.sh

TEST_ID=$(generate_test_id)
echo "TEST_ID = $TEST_ID"

tmp_dir="/tmp/test-$TEST_ID"
mkdir -p $tmp_dir

cleanup() {
    echo "cleanup"
    clean_resource_by_id $TEST_ID
    rm -rf $tmp_dir
}

if [ -z "${TEST_NOCLEANUP:-}" ]; then
    trap cleanup EXIT
else
    log "TEST_NOCLEANUP is set; not cleaning up test artifacts afterwards."
fi

env=binary-$TEST_ID

cd $ROOT/binary/examples

export BINARY_BUILDER_IMAGE=binary-builder
export BINARY_RUNTIME_IMAGE=binary-env

log "Creating environment for Fission binary"
fission env create --name $env --image $BINARY_RUNTIME_IMAGE --builder $BINARY_BUILDER_IMAGE --period 5

timeout 90 bash -c "wait_for_builder $env"

log "===== 1. Test GET ====="

fn_name=hello-binary-$TEST_ID

log "Creating function for $fn_name"
fission fn create --name $fn_name --code hello.sh --env $env

#timeout 90 bash -c "waitBuild $pkgName"

log "Creating route for $fn_name"
fission route create --name $fn_name --function $fn_name --url /$fn_name --method GET

log "Waiting for router & pools to catch up"
sleep 5

log "Testing the $fn_name function"
timeout 60 bash -c "test_fn $fn_name 'Hello'"

log "===== 2. Testing POST ====="

fn_name=echo-binary-$TEST_ID

log "Creating function for $fn_name"
fission fn create --name $fn_name --code echo.sh --env $env

log "Creating route for $fn_name"
fission route create --name $fn_name --function $fn_name --url /$fn_name --method POST

log "Waiting for router & pools to catch up"
sleep 5

log "Testing the $fn_name function"
timeout 60 bash -c "test_post_route $fn_name 'Hello' '... Hello'"

log "===== 3. Testing Module ====="

fn_name=module-binary-$TEST_ID

zip module-example.zip module-example/*

log "Creating package for $fn_name"
fission pkg create --src module-example.zip --name module-example --env $env

log "Creating function for $fn_name"
fission fn create --name $fn_name --env $env --pkg module-example --entry "module-example/test.sh"

log "Creating route for $fn_name"
fission route create --name $fn_name --function $fn_name --url /$fn_name --method GET

log "Waiting for router & pools to catch up"
sleep 5

log "Testing the $fn_name function"
timeout 60 bash -c "test_fn $fn_name 'Modules are awesome'"
