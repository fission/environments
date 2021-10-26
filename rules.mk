# Platforms to build in multi-architecture images.
PLATFORMS ?= linux/amd64,linux/arm64

# Repository prefix and tag to push multi-architecture images to.
REPO ?= fission
TAG ?= dev
DOCKER_FLAGS ?= --push --progress plain
FISSION_VERSION ?= v1.15.0-rc1
SKAFFOLD_PROFILE ?= kind

%-img:
	@echo === Building image $(REPO)/$(subst -img,,$@):$(TAG) using context $(CURDIR) and dockerfile $<
	echo docker buildx build $($@-buildargs) --platform=$(PLATFORMS) -t $(REPO)/$(subst -img,,$@):$(TAG) $(DOCKER_FLAGS) -f $< .

%-builder:
	cd builder/ && $(MAKE)
