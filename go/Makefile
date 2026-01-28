-include ../rules.mk

.PHONY: all
all: go-env-img go-env-1.25-img

go-env-img-buildargs := --build-arg GO_VERSION=1.25 --build-arg UBUNTU_VERSION=22.04

go-env-1.25-img-buildargs := --build-arg GO_VERSION=1.25 --build-arg UBUNTU_VERSION=22.04

go-env-img: Dockerfile-1.1x

go-env-1.25-img: Dockerfile-1.1x
