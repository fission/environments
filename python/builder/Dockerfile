ARG BUILDER_IMAGE=fission/builder
ARG PY_BASE_IMG=3.11-alpine

FROM ${BUILDER_IMAGE}
FROM python:${PY_BASE_IMG}

COPY --from=0 /builder /builder
RUN apk add --update --no-cache python3-dev build-base gcc bash

ADD defaultBuildCmd /usr/local/bin/build

EXPOSE 8001
