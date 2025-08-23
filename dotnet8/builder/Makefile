PLATFORMS ?= linux/amd64,linux/arm64

-include ../../rules.mk

.PHONY: all
all: dotnet8-builder-img

# Override the rule to use parent directory as context
dotnet8-builder-img: Dockerfile
	@echo === Building image $(REPO)/dotnet8-builder:$(TAG) using parent directory as context
	cd .. && docker buildx build --platform=$(PLATFORMS) -t $(REPO)/dotnet8-builder:$(TAG) $(DOCKER_FLAGS) -f builder/Dockerfile . 