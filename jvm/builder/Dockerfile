## Fission builder base image
ARG BUILDER_IMAGE=fission/builder:latest
FROM ${BUILDER_IMAGE} AS builder

## Use eclipse-temurin:25-jdk-alpine - (https://github.com/adoptium/containers)

FROM eclipse-temurin:25-jdk-alpine

## Section copied from the Maven Dockerfile

RUN apk add --no-cache bash procps curl tar openssh-client

LABEL org.opencontainers.image.title="Apache Maven"
LABEL org.opencontainers.image.source=https://github.com/carlossg/docker-maven
LABEL org.opencontainers.image.url=https://github.com/carlossg/docker-maven
LABEL org.opencontainers.image.description="Apache Maven is a software project management and comprehension tool. Based on the concept of a project object model (POM), Maven can manage a project's build, reporting and documentation from a central piece of information."

ENV MAVEN_HOME=/usr/share/maven

COPY --from=maven:3.9.16-eclipse-temurin-25 ${MAVEN_HOME} ${MAVEN_HOME}
COPY --from=maven:3.9.16-eclipse-temurin-25 /usr/local/bin/mvn-entrypoint.sh /usr/local/bin/mvn-entrypoint.sh
COPY --from=maven:3.9.16-eclipse-temurin-25 /usr/share/maven/ref/settings-docker.xml /usr/share/maven/ref/settings-docker.xml

RUN ln -s ${MAVEN_HOME}/bin/mvn /usr/bin/mvn

ARG MAVEN_VERSION=3.9.16
ARG USER_HOME_DIR="/root"
ENV MAVEN_CONFIG="$USER_HOME_DIR/.m2"

## Pre-install fission-java-core into the local Maven repository so user
## function builds can resolve it (it is no longer available remotely).
COPY install-fission-java-core.sh /usr/local/bin/install-fission-java-core
RUN install-fission-java-core

## Fission builder specific section
COPY --from=builder /builder /builder
ADD build.sh /usr/local/bin/build
EXPOSE 8001
