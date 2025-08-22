PLATFORMS ?= linux/amd64

REPO ?= fission
TAG ?= latest

.PHONY: all
all: dotnet8-env-img dotnet8-builder-img

dotnet8-env-img: Dockerfile
	@echo === Building image $(REPO)/dotnet8-env:$(TAG) using context $(CURDIR) and dockerfile $<
	docker build --platform=$(PLATFORMS) -t $(REPO)/dotnet8-env:$(TAG) -f $< .

dotnet8-builder-img: builder/Dockerfile
	@echo === Building image $(REPO)/dotnet8-builder:$(TAG) using context $(CURDIR)/builder and dockerfile $<
	docker build --platform=$(PLATFORMS) -t $(REPO)/dotnet8-builder:$(TAG) -f $< builder/