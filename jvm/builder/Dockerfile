## Fission builder base image
ARG BUILDER_IMAGE=fission/builder:latest
FROM ${BUILDER_IMAGE} AS builder

## Use eclipse-temurin:22-jdk-alpine - (https://github.com/adoptium/containers/blob/07677395574f5d3462c3b6fdf5f6c4a0a350b683/22/jdk/alpine/Dockerfile)

FROM eclipse-temurin:22-jdk-alpine

## Section copied from the Maven Dockerfile

RUN apk add --no-cache bash procps curl tar openssh-client

LABEL org.opencontainers.image.title="Apache Maven"
LABEL org.opencontainers.image.source=https://github.com/carlossg/docker-maven
LABEL org.opencontainers.image.url=https://github.com/carlossg/docker-maven
LABEL org.opencontainers.image.description="Apache Maven is a software project management and comprehension tool. Based on the concept of a project object model (POM), Maven can manage a project's build, reporting and documentation from a central piece of information."

ENV MAVEN_HOME=/usr/share/maven

COPY --from=maven:3.9.9-eclipse-temurin-11 ${MAVEN_HOME} ${MAVEN_HOME}
COPY --from=maven:3.9.9-eclipse-temurin-11 /usr/local/bin/mvn-entrypoint.sh /usr/local/bin/mvn-entrypoint.sh
COPY --from=maven:3.9.9-eclipse-temurin-11 /usr/share/maven/ref/settings-docker.xml /usr/share/maven/ref/settings-docker.xml

RUN ln -s ${MAVEN_HOME}/bin/mvn /usr/bin/mvn

ARG MAVEN_VERSION=3.9.9
ARG USER_HOME_DIR="/root"
ENV MAVEN_CONFIG="$USER_HOME_DIR/.m2"

## Fission builder specific section
COPY --from=builder /builder /builder
ADD build.sh /usr/local/bin/build
EXPOSE 8001
