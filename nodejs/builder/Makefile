-include ../../rules.mk

.PHONY: all
all: node-builder-debian-img node-builder-img node-builder-22-img

node-builder-debian-img-buildargs := --build-arg NODE_BASE_IMG=22.18.0

node-builder-img-buildargs := --build-arg NODE_BASE_IMG=22.18.0-alpine3.21

node-builder-22-img-buildargs := --build-arg NODE_BASE_IMG=22.18.0-alpine3.21

node-builder-debian-img: Dockerfile

node-builder-img: Dockerfile

node-builder-22-img: Dockerfile
