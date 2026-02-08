-include ../rules.mk

.PHONY: all
all: bun-builder bun-env-img bun-env-debian-img bun-env-1140-img

bun-env-img-buildargs := --build-arg BUN_BASE_IMG=1.1.40-alpine

bun-env-debian-img-buildargs := --build-arg BUN_BASE_IMG=1.1.40

bun-env-1140-img-buildargs := --build-arg BUN_BASE_IMG=1.1.40-alpine

bun-env-img: Dockerfile

bun-env-debian-img: Dockerfile

bun-env-1140-img: Dockerfile