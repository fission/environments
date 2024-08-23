-include ../../rules.mk

.PHONY: all
all: python-builder-img python-builder-3.12-img

python-builder-img-buildargs := --build-arg PY_BASE_IMG=3.11-alpine

python-builder-3.12-img-buildargs := --build-arg PY_BASE_IMG=3.12-alpine

python-builder-img: Dockerfile

python-builder-3.12-img: Dockerfile