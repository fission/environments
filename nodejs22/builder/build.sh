#!/bin/sh
cd ${SRC_PKG}
if [[ -n "$NPM_TOKEN" ]] && [[ -n "$NPM_REGISTRY" ]]; then
    npm set //${NPM_REGISTRY}/:_authToken ${NPM_TOKEN}
fi
npm install && cp -r ${SRC_PKG} ${DEPLOY_PKG}
