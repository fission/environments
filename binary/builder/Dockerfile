ARG BUILDER_IMAGE=fission/builder:latest
FROM ${BUILDER_IMAGE}

FROM alpine:3.20

COPY --from=0 /builder /builder

ADD build.sh /usr/local/bin/build