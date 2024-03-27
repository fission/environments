#!/bin/sh
cpanm -n $(cat ${SRC_PKG}/modules) -l ${SRC_PKG} && cp -r ${SRC_PKG} ${DEPLOY_PKG}


