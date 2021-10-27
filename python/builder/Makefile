-include ../../rules.mk

.PHONY: all
all: python-builder-img python-builder-3.7-img python-builder-3.8-img python-builder-3.9-img python-builder-3.10-img

python-builder-img-buildargs := --build-arg PY_BASE_IMG=3.7-alpine

python-builder-3.7-img-buildargs := --build-arg PY_BASE_IMG=3.7-alpine

python-builder-3.8-img-buildargs := --build-arg PY_BASE_IMG=3.8-alpine

python-builder-3.9-img-buildargs := --build-arg PY_BASE_IMG=3.9-alpine

python-builder-3.10-img-buildargs := --build-arg PY_BASE_IMG=3.10-alpine

python-builder-img: Dockerfile

python-builder-3.7-img: Dockerfile

python-builder-3.8-img: Dockerfile

python-builder-3.9-img: Dockerfile

python-builder-3.10-img: Dockerfile