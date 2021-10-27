ARG BUILDER_IMAGE=fission/builder
ARG PY_BASE_IMG

FROM ${BUILDER_IMAGE}
FROM python:${PY_BASE_IMG}

COPY --from=0 /builder /builder
RUN apk update
RUN apk add --no-cache python3-dev build-base gcc bash
RUN rm -r /root/.cache

ADD defaultBuildCmd /usr/local/bin/build

EXPOSE 8001
