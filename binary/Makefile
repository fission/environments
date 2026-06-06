-include ../rules.mk

.PHONY: all
all: binary-builder binary-env-img

binary-env-img-buildargs := --build-arg GO_BASE_IMAGE=1.26-alpine --build-arg ALPINE_VERSION=3.22

binary-env-img: Dockerfile