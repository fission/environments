FROM maven:3.9.16-eclipse-temurin-25-alpine AS build
WORKDIR /usr/src/myapp/

# fission-java-core is no longer resolvable from any remote repository,
# so build and install it from source first.
COPY install-fission-java-core.sh /usr/local/bin/install-fission-java-core
RUN install-fission-java-core

# To reuse the build cache, here we split maven dependency
# download and package into two RUN commands to avoid cache invalidation.
COPY pom.xml .
RUN mvn dependency:go-offline

COPY src /usr/src/myapp/src/
RUN mvn package

FROM eclipse-temurin:25-jdk-alpine
VOLUME /tmp
COPY --from=build /usr/src/myapp/target/env-java-0.0.1-SNAPSHOT.jar /app.jar
ENTRYPOINT java ${JVM_OPTS} -Djava.security.egd=file:/dev/./urandom -jar /app.jar --server.port=8888
EXPOSE 8888
