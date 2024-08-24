-include ../rules.mk

.PHONY: all
all: binary-builder binary-env-img

binary-env-img-buildargs := --build-arg GO_BASE_IMAGE=1.22-alpine --build-arg ALPINE_VERSION=3.20

binary-env-img: Dockerfile