-include ../rules.mk

.PHONY: all
all: python-env-img python-env-3.12-img

python-env-img-buildargs := --build-arg PY_BASE_IMG=3.11-alpine

python-env-3.12-img-buildargs := --build-arg PY_BASE_IMG=3.12-alpine

python-env-img: Dockerfile

python-env-3.12-img: Dockerfile
