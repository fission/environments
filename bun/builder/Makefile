-include ../../rules.mk

.PHONY: all
all: bun-builder-debian-img bun-builder-img bun-builder-1140-img

bun-builder-debian-img-buildargs := --build-arg BUN_BASE_IMG=1.1.40

bun-builder-img-buildargs := --build-arg BUN_BASE_IMG=1.1.40-alpine

bun-builder-1140-img-buildargs := --build-arg BUN_BASE_IMG=1.1.40-alpine

bun-builder-debian-img: Dockerfile

bun-builder-img: Dockerfile

bun-builder-1140-img: Dockerfile
