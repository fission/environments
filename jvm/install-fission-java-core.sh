#!/bin/sh
#
# Installs io.fission:fission-java-core into the local Maven repository by
# building it from source (a pinned commit of fission/fission-java-libs).
#
# Background: fission-java-core was only ever published as a SNAPSHOT to
# oss.sonatype.org (OSSRH), which was decommissioned in 2025, so the artifact
# can no longer be resolved from any remote repository.
#
# The artifact is installed under several versions so that both the poms in
# this repository (0.0.2) and pre-existing user functions that still
# reference 0.0.2-SNAPSHOT resolve from the local repository.
set -eu

COMMIT=e97baa8b6a306a552dc53d576075e7c410bcfaa4
VERSIONS="0.0.1 0.0.2 0.0.2-SNAPSHOT"

workdir=$(mktemp -d)
trap 'rm -rf "$workdir"' EXIT

wget -qO "$workdir/src.tar.gz" "https://github.com/fission/fission-java-libs/archive/${COMMIT}.tar.gz"
tar -xzf "$workdir/src.tar.gz" -C "$workdir"
cd "$workdir/fission-java-libs-${COMMIT}/fission-java-core"

# The pom is missing its XML namespace declarations, which strict plugin
# parsers (e.g. versions-maven-plugin) reject; normalize the root element.
sed -i '1s|^<project[^>]*>|<project xmlns="http://maven.apache.org/POM/4.0.0" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://maven.apache.org/POM/4.0.0 http://maven.apache.org/xsd/maven-4.0.0.xsd">|' pom.xml

for version in $VERSIONS; do
    mvn -B -q versions:set -DnewVersion="$version"
    mvn -B -q -DskipTests -Dmaven.compiler.release=17 install
done
