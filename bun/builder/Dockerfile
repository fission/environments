ARG BUN_BASE_IMG
ARG BUILDER_IMAGE=fission/builder:latest

FROM ${BUILDER_IMAGE}

FROM oven/bun:${BUN_BASE_IMG}

ARG BUN_ENV
ENV BUN_ENV $BUN_ENV

COPY --from=0 /builder /builder
ADD build.sh /usr/local/bin/build
RUN chmod +x /usr/local/bin/build
