PLATFORMS ?= linux/amd64

-include ../rules.mk

REPO ?= fission
TAG ?= latest

.PHONY: all
all: dotnet8-env-img dotnet8-builder-img

dotnet8-env-img: Dockerfile
	@echo === Building image $(REPO)/dotnet8-env:$(TAG) using context $(CURDIR) and dockerfile $<
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/dotnet8-env:$(TAG) $(DOCKER_FLAGS) -f $< .

dotnet8-builder-img: builder/Dockerfile
	@echo === Building image $(REPO)/dotnet8-builder:$(TAG) using context $(CURDIR) and dockerfile $<
	docker build --platform=$(PLATFORMS) -t $(REPO)/dotnet8-builder:$(TAG) -f $< .