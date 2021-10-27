-include ../rules.mk

.PHONY: all
all: python-builder python-env-img python-env-3.7-img python-env-3.8-img python-env-3.9-img python-env-3.10-img

python-env-img-buildargs := --build-arg PY_BASE_IMG=3.7-alpine

python-env-3.7-img-buildargs := --build-arg PY_BASE_IMG=3.7-alpine

python-env-3.8-img-buildargs := --build-arg PY_BASE_IMG=3.8-alpine

python-env-3.9-img-buildargs := --build-arg PY_BASE_IMG=3.9-alpine

python-env-3.10-img-buildargs := --build-arg PY_BASE_IMG=3.10-alpine

python-env-img: Dockerfile

python-env-3.7-img: Dockerfile

python-env-3.8-img: Dockerfile

python-env-3.9-img: Dockerfile

python-env-3.10-img: Dockerfile
