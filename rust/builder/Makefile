PLATFORMS ?= linux/amd64,linux/arm64

-include ../../rules.mk

.PHONY: all
all: rust-builder-img

rust-builder-img-buildargs := --build-arg RUST_VERSION=1.96 --build-arg DEBIAN_VERSION=bookworm

# Override the implicit rule: the build context is the parent directory
# so the Dockerfile can vendor the workspace (SDK + template crates).
rust-builder-img: Dockerfile
	@echo === Building image $(REPO)/rust-builder:$(TAG) using parent directory as context
	cd .. && docker buildx build $(rust-builder-img-buildargs) --platform=$(PLATFORMS) -t $(REPO)/rust-builder:$(TAG) $(DOCKER_FLAGS) -f builder/Dockerfile .
