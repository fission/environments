-include ../rules.mk

.PHONY: all
all: go-env-img go-env-1.17-img go-env-1.16-img go-env-1.15-img go-env-1.14-img

go-env-img-buildargs := --build-arg GO_VERSION=1.15 --build-arg UBUNTU_VERSION=20.04

go-env-1.17-img-buildargs := --build-arg GO_VERSION=1.17 --build-arg UBUNTU_VERSION=20.04

go-env-1.16-img-buildargs := --build-arg GO_VERSION=1.16 --build-arg UBUNTU_VERSION=20.04

go-env-1.15-img-buildargs := --build-arg GO_VERSION=1.15  --build-arg UBUNTU_VERSION=20.04

go-env-1.14-img-buildargs := --build-arg GO_VERSION=1.14  --build-arg UBUNTU_VERSION=20.04

go-env-1.13-img-buildargs := --build-arg GO_VERSION=1.13  --build-arg UBUNTU_VERSION=20.04

go-env-img: Dockerfile-1.1x

# go-env-1.17-img: Dockerfile-1.1x
go-env-1.17-img: Dockerfile-1.1x
	@echo $@
	@echo === Building image $(REPO)/$(subst -img,,$@):$(TAG) using context $(CURDIR) and dockerfile $<
	@echo docker buildx build $($@-buildargs) --platform=$(PLATFORMS) -t $(REPO)/$(subst -img,,$@):$(TAG) $(DOCKER_FLAGS) -f $< .

go-env-1.16-img: Dockerfile-1.1x

go-env-1.15-img: Dockerfile-1.1x

go-env-1.14-img: Dockerfile-1.1x

go-env-1.13-img: Dockerfile-1.1x
