-include ../../rules.mk

.PHONY: all
all: go-builder-img go-builder-1.23-img

go-builder-img-buildargs := --build-arg GO_VERSION=1.22

go-builder-1.23-img-buildargs := --build-arg GO_VERSION=1.23

go-builder-img: Dockerfile-1.1x

go-builder-1.23-img: Dockerfile-1.1x
