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

env=rust-$TEST_ID
fn_poolmgr=hello-rust-poolmgr-$TEST_ID
fn_nd=hello-rust-nd-$TEST_ID
fn_echo=echo-rust-$TEST_ID

cd $ROOT/rust/examples

export RUST_BUILDER_IMAGE=rust-builder
export RUST_RUNTIME_IMAGE=rust-env

log "Creating environment for Rust"
fission env create --name $env --image $RUST_RUNTIME_IMAGE --builder $RUST_BUILDER_IMAGE --period 5

timeout 90 bash -c "wait_for_builder $env"

log "Creating single-file (template mode) package"
pkgName=$(generate_test_id)
fission package create --name $pkgName --src hello.rs --env $env

# wait for build to finish at most 120s
timeout 120 bash -c "waitBuild $pkgName"

log "Creating pool manager & new deployment function for Rust"
fission fn create --name $fn_poolmgr --env $env --pkg $pkgName
fission fn create --name $fn_nd      --env $env --pkg $pkgName --executortype newdeploy

log "Creating routes for pool manager & new deployment function"
fission route create --name $fn_poolmgr --function $fn_poolmgr --url /$fn_poolmgr --method GET
fission route create --name $fn_nd --function $fn_nd --url /$fn_nd --method GET

log "Waiting for router & pools to catch up"
sleep 5

log "Testing pool manager function"
timeout 60 bash -c "test_fn $fn_poolmgr 'Hello, World!'"

log "Testing new deployment function"
timeout 60 bash -c "test_fn $fn_nd 'Hello, World!'"

log "Creating Cargo project (project mode) package"
# Create zip file without top level directory
(cd project-example && zip -r $tmp_dir/project.zip Cargo.toml src)

pkgName=$(generate_test_id)
fission package create --name $pkgName --src $tmp_dir/project.zip --env $env

# first project build downloads crates; allow more time
timeout 300 bash -c "waitBuild $pkgName"

log "Creating echo function from the Cargo project"
fission fn create --name $fn_echo --env $env --pkg $pkgName
fission route create --name $fn_echo --function $fn_echo --url /$fn_echo --method POST

log "Waiting for router to catch up"
sleep 5

log "Testing POST body passthrough"
timeout 60 bash -c "test_post_route $fn_echo '{\"lang\":\"rust\"}' 'echo.*lang.*rust'"

log "Test PASSED"
