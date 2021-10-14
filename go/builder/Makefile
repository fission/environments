-include ../../rules.mk

.PHONY: all
all: go-builder-img go-builder-1.17-img go-builder-1.16-img go-builder-1.15-img go-builder-1.14-img

go-builder-img-buildargs := "--build-arg GO_VERSION=1.15"

go-builder-1.17-img-buildargs := "--build-arg GO_VERSION=1.17"

go-builder-1.16-img-buildargs := "--build-arg GO_VERSION=1.16"

go-builder-1.15-img-buildargs := "--build-arg GO_VERSION=1.15"

go-builder-1.14-img-buildargs := "--build-arg GO_VERSION=1.14"

go-builder-1.13-img-buildargs := "--build-arg GO_VERSION=1.13"

go-builder-img: Dockerfile-1.1x

go-builder-1.17-img: Dockerfile-1.1x

go-builder-1.16-img: Dockerfile-1.1x

go-builder-1.15-img: Dockerfile-1.1x

go-builder-1.14-img: Dockerfile-1.1x

go-builder-1.13-img: Dockerfile-1.1x
