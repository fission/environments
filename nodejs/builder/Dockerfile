ARG NODE_BASE_IMG
ARG BUILDER_IMAGE=fission/builder:latest

FROM ${BUILDER_IMAGE}

FROM node:${NODE_BASE_IMG}

ARG NODE_ENV
ENV NODE_ENV $NODE_ENV

COPY --from=0 /builder /builder
ADD build.sh /usr/local/bin/build
RUN chmod +x /usr/local/bin/build
