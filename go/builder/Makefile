-include ../../rules.mk

.PHONY: all
all: go-builder-img go-builder-1.22-img go-builder-1.21-img go-builder-1.20-img 

go-builder-img-buildargs := --build-arg GO_VERSION=1.20

go-builder-1.22-img-buildargs := --build-arg GO_VERSION=1.22

go-builder-1.21-img-buildargs := --build-arg GO_VERSION=1.21

go-builder-1.20-img-buildargs := --build-arg GO_VERSION=1.20

go-builder-img: Dockerfile-1.1x

go-builder-1.22-img: Dockerfile-1.1x

go-builder-1.21-img: Dockerfile-1.1x

go-builder-1.20-img: Dockerfile-1.1x

