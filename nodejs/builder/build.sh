#!/bin/sh
cd ${SRC_PKG}
npm install --production && cp -r ${SRC_PKG} ${DEPLOY_PKG}
