-include ../rules.mk

.PHONY: all
all: node-env-debian-img node-env-16-img

node-env-debian-img-buildargs := --build-arg NODE_BASE_IMG=12.22.7

node-env-16-img-buildargs := --build-arg NODE_BASE_IMG=16.12.0-alpine3.14

node-env-debian-img: Dockerfile

node-env-16-img: Dockerfile