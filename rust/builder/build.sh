#!/bin/sh

if [ ! -d ${SRC_PKG} ]; then
	echo "Please specify the rust project folder"
	exit 1
fi

folder_name=`ls -d *`
# Assume that there is only one folder in SRC_PKG
cargo install --path ${SRC_PKG}/$folder_name
mkdir -p ${DEPLOY_PKG}
cp -rf ${SRC_PKG}/$folder_name/target/release/$folder_name ${DEPLOY_PKG}/$folder_name
