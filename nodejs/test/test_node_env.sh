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
    # Clean up configmaps
    kubectl delete configmap $env_v1api-config --ignore-not-found=true
    kubectl delete configmap $env_v2api-config --ignore-not-found=true
    rm -rf $tmp_dir
}

if [ -z "${TEST_NOCLEANUP:-}" ]; then
    trap cleanup EXIT
else
    log "TEST_NOCLEANUP is set; not cleaning up test artifacts afterwards."
fi

NODE_RUNTIME_IMAGE=node-env
NODE_BUILDER_IMAGE=node-builder

env_v1api=nodejs-v1-$TEST_ID
env_v2api=nodejs-v2-$TEST_ID
fn1=test-nodejs-env-1-$TEST_ID
fn2=test-nodejs-env-2-$TEST_ID
fn3=test-nodejs-env-3-$TEST_ID
fn4=test-nodejs-env-4-$TEST_ID
fn5=test-nodejs-env-5-$TEST_ID
fn6=test-nodejs-env-6-$TEST_ID

test_path=$ROOT/nodejs/test

log "Creating configmaps for CJS and ESM modes..."
# Create configmap for CJS mode (LOAD_ESM=false)
kubectl create configmap $env_v1api-config --from-literal=LOAD_ESM=false
# Create configmap for ESM mode (LOAD_ESM=true)  
kubectl create configmap $env_v2api-config --from-literal=LOAD_ESM=true

log "Creating v1api environment ..."
log "NODE_RUNTIME_IMAGE = $NODE_RUNTIME_IMAGE"
fission env create \
    --name $env_v1api \
    --image $NODE_RUNTIME_IMAGE \

log "Creating v2api environment ..."
log "NODE_RUNTIME_IMAGE = $NODE_RUNTIME_IMAGE     NODE_BUILDER_IMAGE = $NODE_BUILDER_IMAGE"
fission env create \
    --name $env_v2api \
    --image $NODE_RUNTIME_IMAGE \
    --builder $NODE_BUILDER_IMAGE
timeout 180s bash -c "wait_for_builder $env_v2api"


log "===== 1. test env with v1 api (CJS) ====="
fission fn create --name $fn1 --env $env_v1api --code $test_path/test-case-1/helloWorld.js --configmap $env_v1api-config

fission route create --name $fn1 --function $fn1 --url /$fn1 --method GET
sleep 3     # Waiting for router to catch up
timeout 60 bash -c "test_fn $fn1 \"hello, world!\""

log "===== 2. test query string (CJS) ====="
fission fn create --name $fn2 --env $env_v1api --code $test_path/test-case-2/helloUser.js --configmap $env_v1api-config

fission route create --name $fn2 --function $fn2 --url /$fn2 --method GET
sleep 3     # Waiting for router to catch up
timeout 60 bash -c "test_fn $fn2?user=foo \"hello foo!\""

log "===== 3. test POST (CJS) ====="
fission fn create --name $fn3 --env $env_v1api --code $test_path/test-case-3/wordCount.js --configmap $env_v1api-config

fission route create --name $fn3 --function $fn3 --url /$fn3 --method POST
sleep 3     # Waiting for router to catch up
body='Its a beautiful day'
timeout 20 bash -c "test_post_route $fn3 $body 4"

log "===== 4. test builder (CJS with dependencies) ====="
log "Creating package ..."
pushd $test_path/test-case-4
zip -r $tmp_dir/src-pkg.zip momentExample.js package.json
popd
pkg=$(generate_test_id)
fission package create --name $pkg --src $tmp_dir/src-pkg.zip --env $env_v2api 
timeout 60s bash -c "waitBuild $pkg"

fission fn create --name $fn4 --pkg $pkg --env $env_v2api --entrypoint "momentExample" --configmap $env_v1api-config

fission route create --function $fn4 --url /$fn4 --method GET
sleep 3     # Waiting for router to catch up
timeout 60 bash -c "test_fn $fn4 'Hello'"

log "===== 5. test ESM helloWorld ====="
fission fn create --name $fn5 --env $env_v2api --code $test_path/test-case-5/helloWorld.js --configmap $env_v2api-config

fission route create --name $fn5 --function $fn5 --url /$fn5 --method GET
sleep 3     # Waiting for router to catch up
timeout 60 bash -c "test_fn $fn5 \"hello, world from ESM!\""

log "===== 6. test ESM helloUser ====="
fission fn create --name $fn6 --env $env_v2api --code $test_path/test-case-6/helloUser.js --configmap $env_v2api-config

fission route create --name $fn6 --function $fn6 --url /$fn6 --method GET  
sleep 3     # Waiting for router to catch up
timeout 60 bash -c "test_fn $fn6?user=foo \"hello foo from ESM!\""

log "Test PASSED - Both CJS and ESM functions working!"
