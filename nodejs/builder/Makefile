-include ../../rules.mk

.PHONY: all
all: node-builder-debian-img node-builder-16-img

node-builder-debian-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7

node-builder-16-img-buildargs := --build-arg NODE_BASE_IMG=16.12.0-alpine3.14

node-builder-debian-img: Dockerfile

node-builder-16-img: Dockerfile
