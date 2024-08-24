-include ../rules.mk

.PHONY: all
all: go-env-img go-env-1.23-img

go-env-img-buildargs := --build-arg GO_VERSION=1.22 --build-arg UBUNTU_VERSION=22.04

go-env-1.23-img-buildargs := --build-arg GO_VERSION=1.23 --build-arg UBUNTU_VERSION=22.04

go-env-img: Dockerfile-1.1x

go-env-1.23-img: Dockerfile-1.1x

