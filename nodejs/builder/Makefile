-include ../../rules.mk

.PHONY: all
all: node-builder-img node-builder-debian-img node-builder-12-img node-builder-14-img node-builder-16-img

node-builder-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7-alpine3.14

node-builder-debian-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7

node-builder-12-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7-alpine3.14

node-builder-14-img-buildargs := --build-arg NODE_BASE_IMG=14.18.1-alpine3.14

node-builder-16-img-buildargs := --build-arg NODE_BASE_IMG=16.12.0-alpine3.14

node-builder-img: Dockerfile

node-builder-debian-img: Dockerfile

node-builder-12-img: Dockerfile

node-builder-14-img: Dockerfile

node-builder-16-img: Dockerfile
