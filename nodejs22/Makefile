-include ../rules.mk

.PHONY: all
all: node-builder node-env-img node-env-debian-img node-env-22-img

node-env-img-buildargs := --build-arg NODE_BASE_IMG=20.16.0-alpine3.20

node-env-debian-img-buildargs := --build-arg NODE_BASE_IMG=20.16.0

node-env-22-img-buildargs := --build-arg NODE_BASE_IMG=22.17.1-alpine3.22

node-env-img: Dockerfile

node-env-debian-img: Dockerfile

node-env-22-img: Dockerfile