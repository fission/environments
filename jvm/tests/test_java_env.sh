#!/bin/bash

set -euo pipefail
source test_utils/utils.sh

# Use the locally built image loaded into kind by `make jvm-test-images`
# (the utils.sh default points at the stale Docker Hub image), matching the
# convention of the go/binary/nodejs/python tests.
export JVM_RUNTIME_IMAGE=jvm-env

TEST_ID=$(generate_test_id)
echo "TEST_ID = $TEST_ID"

env=jvm-$TEST_ID
fn_n=jvm-hello-n-$TEST_ID
fn_p=jvm-hello-p-$TEST_ID

cleanup() {
    clean_resource_by_id $TEST_ID
}

if [ -z "${TEST_NOCLEANUP:-}" ]; then
    trap cleanup EXIT
else
    log "TEST_NOCLEANUP is set; not cleaning up test artifacts afterwards."
fi

cd jvm/tests/fixtures/java

log "Creating the jar from application"
#Using Docker to build Jar so that maven & other Java dependencies are not needed on CI server
#fission-java-core must be installed into the container-local Maven repository
#first because it is not resolvable from any remote repository.
docker run --rm \
    -v "$(pwd)":/usr/src/mymaven \
    -v "$(pwd)/../../../install-fission-java-core.sh":/usr/local/bin/install-fission-java-core \
    -w /usr/src/mymaven \
    maven:3.9.16-eclipse-temurin-25-alpine \
    sh -c "install-fission-java-core && mvn clean package -q"

log "Creating environment for Java"
fission env create --name $env --image $JVM_RUNTIME_IMAGE --version 2 --keeparchive=true

log "Creating pool manager & new deployment function for Java"
fission fn create --name $fn_p --deploy target/hello-world-1.0-SNAPSHOT-jar-with-dependencies.jar --env $env --entrypoint io.fission.HelloWorld
fission fn create --name $fn_n --deploy target/hello-world-1.0-SNAPSHOT-jar-with-dependencies.jar --env $env --executortype newdeploy --entrypoint io.fission.HelloWorld

log "Creating route for pool manager function"
fission route create --name $fn_p --function $fn_p --url /$fn_p --method GET

log "Creating route for new deployment function"
fission route create --name $fn_n --function $fn_n --url /$fn_n --method GET

log "Waiting for router & pools to catch up"
sleep 5

log "Testing pool manager function"
timeout 60 bash -c "test_fn $fn_p 'Hello'"

log "Testing new deployment function"
timeout 60 bash -c "test_fn $fn_n 'Hello'"

log "Test PASSED"
