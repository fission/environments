-include ../rules.mk

.PHONY: all
all: node-builder node-env-img node-env-debian-img node-env-12-img node-env-14-img node-env-16-img

node-env-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7-alpine3.14

node-env-debian-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7

node-env-12-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7-alpine3.14

node-env-14-img-buildargs := --build-arg NODE_BASE_IMG=14.18.1-alpine3.14

node-env-16-img-buildargs := --build-arg NODE_BASE_IMG=16.12.0-alpine3.14

node-env-img: Dockerfile

node-env-debian-img: Dockerfile

node-env-12-img: Dockerfile

node-env-14-img: Dockerfile

node-env-16-img: Dockerfile