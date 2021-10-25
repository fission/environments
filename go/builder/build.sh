#!/bin/bash

set -eux

srcDir=${GOPATH}/src/$(basename ${SRC_PKG})

trap "rm -rf ${srcDir}" EXIT

# http://ask.xmodulo.com/compare-two-version-numbers.html
version_ge() { test "$(echo "$@" | tr " " "\n" | sort -rV | head -n 1)" == "$1"; }

if [ -d ${SRC_PKG} ]; then
    echo "Building in directory ${srcDir}"
    ln -sf ${SRC_PKG} ${srcDir}
elif [ -f ${SRC_PKG} ]; then
    echo "Building file ${SRC_PKG} in ${srcDir}"
    mkdir -p ${srcDir}
    cp ${SRC_PKG} ${srcDir}/function.go
fi

cd ${srcDir}

if [ ! -z ${GOLANG_VERSION} ] && version_ge ${GOLANG_VERSION} "1.12"; then
    if [ -f "go.mod" ]; then
        go mod download
    else
        # Since we're in GOPATH, we need to enable this
        export GO111MODULE="on"
        # still need to do this; otherwise, go will complain "cannot find main module".
        go mod init
        go mod tidy
    fi
else # go version lower than go 1.12
    if [ -f "go.mod" ]; then
        echo "Please update fission/go-builder and fission/go-env image to the latest version to support go module"
        exit 1
    fi
fi

GOFLAGS="-buildmode=plugin"

# use vendor mode if the vendor dir exists when go version is greater
# than 1.12 (the version that fission started to support go module).
if [ -d "vendor" ] && [ ! -z ${GOLANG_VERSION} ] && version_ge ${GOLANG_VERSION} "1.12"; then
    GOFLAGS="${GOFLAGS} -mod=vendor"
fi

# -i is deprecated in go 1.15+
if ! version_ge ${GOLANG_VERSION} "1.15"; then
    GOFLAGS="${GOFLAGS} -i"
fi

go build ${GOFLAGS} -o ${DEPLOY_PKG} .