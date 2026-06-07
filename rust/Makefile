-include ../rules.mk

.PHONY: all
all: rust-builder rust-env-img

rust-env-img-buildargs := --build-arg RUST_VERSION=1.96 --build-arg DEBIAN_VERSION=bookworm

rust-env-img: Dockerfile
