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
fn_poolmgr=binary-poolmgr-$TEST_ID
fn_nd=binary-nd-$TEST_ID

cd $ROOT/binary/examples

export BINARY_BUILDER_IMAGE=binary-builder
export BINARY_RUNTIME_IMAGE=binary-env

log "Creating environment for Fission binary"
fission env create --name $env --image $BINARY_RUNTIME_IMAGE --builder $BINARY_BUILDER_IMAGE --period 5

timeout 90 bash -c "wait_for_builder $env"

pkgName=$(generate_test_id)
fission package create --name $pkgName --src hello.sh --env $env

# wait for build to finish at most 90s
timeout 90 bash -c "waitBuild $pkgName"

log "Creating pool manager & new deployment function for fission binary"
fission fn create --name $fn_poolmgr --env $env --pkg $pkgName
fission fn create --name $fn_nd      --env $env --pkg $pkgName

log "Creating route for new deployment function"
fission route create --name $fn_poolmgr --function $fn_poolmgr --url /$fn_poolmgr --method GET
fission route create --name $fn_nd --function $fn_nd --url /$fn_nd --method GET

log "Waiting for router & pools to catch up"
sleep 5

log "Testing pool manager function"
timeout 60 bash -c "test_fn $fn_poolmgr 'Hello'"

log "Testing new deployment function"
timeout 60 bash -c "test_fn $fn_nd 'Hello'"

# Create zip file without top level directory (module-example)
cd module-example && zip -0 -r $tmp_dir/module.zip * -x "*README*"

pkgName=$(generate_test_id)
fission package create --name $pkgName --src $tmp_dir/module.zip --env $env

# wait for build to finish at most 90s
timeout 90 bash -c "waitBuild $pkgName"

log "Update function package"
fission fn update --name $fn_poolmgr --pkg $pkgName
fission fn update --name $fn_nd --pkg $pkgName

log "Waiting for router & pools to catch up"
sleep 5

log "Testing pool manager function with new package"
timeout 60 bash -c "test_fn $fn_poolmgr 'Vendor'"

log "Testing new deployment function with new package"
timeout 60 bash -c "test_fn $fn_nd 'Vendor'"

log "Test PASSED"

