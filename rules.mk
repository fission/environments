# Platforms to build in multi-architecture images.
PLATFORMS ?= linux/amd64,linux/arm64

# Repository prefix and tag to push multi-architecture images to.
REPO ?= fission
TAG ?= dev
DOCKER_FLAGS ?= --push --progress plain
FISSION_VERSION ?= 1.14.1
SKAFFOLD_PROFILE ?= kind

%-img:
	@echo === Building image $(REPO)/$(subst -img,,$@):$(TAG) using context $(CURDIR) and dockerfile $<
	docker buildx build $($@-buildargs) --platform=$(PLATFORMS) -t $(REPO)/$(subst -img,,$@):$(TAG) $(DOCKER_FLAGS) -f $< .

%-builder:
	cd builder/ && $(MAKE)

install-skaffold:
	@echo === Installing skaffold
	curl -Lo skaffold https://storage.googleapis.com/skaffold/releases/latest/skaffold-linux-amd64 && \
    sudo install skaffold /usr/local/bin/ && \
    skaffold version

install-fission-cli:
	@echo === Installing fission cli
	curl -Lo fission https://github.com/fission/fission/releases/download/$(FISSION_VERSION)/fission-$(FISSION_VERSION)-linux-amd64 && \
	chmod +x fission && \
	sudo mv fission /usr/local/bin/

create-crds:
	@echo === Creating fission crds
	kubectl create -k "github.com/fission/fission/crds/v1?ref=$(FISSION_VERSION)"

verify-kind-cluster:
	@echo === Verifying kind cluster
	kubectl cluster-info --context kind-kind
	kubectl get nodes

skaffold-run:
	@echo === Running skaffold
	skaffold run -p $(SKAFFOLD_PROFILE) --tag latest

go-test-images:
	kind load docker-image go-env
	kind load docker-image go-builder

nodejs-test-images:
	kind load docker-image node-env
	kind load docker-image node-builder

router-port-forward:
	kubectl port-forward svc/router 8888:80 -nfission &