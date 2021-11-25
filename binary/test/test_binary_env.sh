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

log "Creating function for fission binary"
fission fn create --name $fn_name --code hello.sh --env $env

#timeout 90 bash -c "waitBuild $pkgName"

log "Creating route for new deployment function"
fission route create --function $fn_name --url /$fn_name --method GET

log "Waiting for router & pools to catch up"
sleep 5

log "Testing the function"
timeout 60 bash -c "test_fn $fn_name 'Hello'"


log "===== 2. Testing POST ====="

fn_name=echo-binary-$TEST_ID

log "Creating function for fission binary"
fission fn create --name $fn_name --code echo.sh --env $env

log "Creating route for new deployment function"
fission route create --function $fn_name --url /$fn_name --method POST

log "Waiting for router & pools to catch up"
sleep 5

log "Testing pool manager function"
timeout 60 bash -c "test_post_route $fn_name 'Hello' '... Hello'"
