FROM maven:3.9.9-eclipse-temurin-22-alpine AS build
WORKDIR /usr/src/myapp/

# To reuse the build cache, here we split maven dependency
# download and package into two RUN commands to avoid cache invalidation.
COPY pom.xml .
RUN mvn dependency:go-offline

COPY src /usr/src/myapp/src/
RUN mvn package

FROM eclipse-temurin:22-jdk-alpine
VOLUME /tmp
COPY --from=build /usr/src/myapp/target/env-java-0.0.1-SNAPSHOT.jar /app.jar
ENTRYPOINT java ${JVM_OPTS} -Djava.security.egd=file:/dev/./urandom -jar /app.jar --server.port=8888
EXPOSE 8888