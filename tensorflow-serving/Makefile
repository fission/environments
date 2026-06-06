-include ../rules.mk

.PHONY: all
all: tensorflow-serving-env-img

# tensorflow/serving images are published for linux/amd64 only.
tensorflow-serving-env-img: PLATFORMS=linux/amd64
tensorflow-serving-env-img: Dockerfile