-include ../../rules.mk

.PHONY: all
all: go-builder-img go-builder-1.26-img

go-builder-img-buildargs := --build-arg GO_VERSION=1.26

go-builder-1.26-img-buildargs := --build-arg GO_VERSION=1.26

go-builder-img: Dockerfile-1.1x

go-builder-1.26-img: Dockerfile-1.1x