ARG BUILDER_IMAGE=fission/builder:latest
FROM ${BUILDER_IMAGE}

FROM ruby:3.4-alpine
COPY --from=0 /builder /builder

RUN apk update
RUN apk add --no-cache build-base

ADD defaultBuildCmd /usr/local/bin/build

EXPOSE 8001
