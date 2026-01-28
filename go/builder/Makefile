-include ../../rules.mk

.PHONY: all
all: go-builder-img go-builder-1.25-img

go-builder-img-buildargs := --build-arg GO_VERSION=1.25

go-builder-1.25-img-buildargs := --build-arg GO_VERSION=1.25

go-builder-img: Dockerfile-1.1x

go-builder-1.25-img: Dockerfile-1.1x