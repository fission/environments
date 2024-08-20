-include ../rules.mk

.PHONY: all
all: go-env-1.22-img

go-env-1.22-img-buildargs := --build-arg GO_VERSION=1.22 --build-arg UBUNTU_VERSION=22.04

go-env-1.22-img: Dockerfile-1.1x

