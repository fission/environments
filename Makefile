# Copyright 2021 The Fission Authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

-include rules.mk

FISSION_ENVS := nodejs-envs \
	go-envs \
	python-envs \
	python-fastapi-envs \
	perl-envs \
	jvm-envs \
	php7-envs \
	dotnet-envs \
	jvm-jersey-envs \
	binary-envs \
	tensorflow-serving-envs \
	dotnet20-envs \
	ruby-envs

all: $(FISSION_ENVS)

verify-builder:
	@./hack/buildx.sh $(PLATFORMS)

$(FISSION_ENVS): verify-builder
	cd $(subst -envs,,$@)/ && $(MAKE)

sort-env-jsons:
	./hack/sort-json.sh

update-env-json: sort-env-jsons
	./hack/merge-json.sh

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
	@skaffold run -p $(SKAFFOLD_PROFILE) --tag latest

binary-test-images:
	@kind load docker-image binary-env
	@kind load docker-image binary-builder

go-test-images:
	@kind load docker-image go-env
	@kind load docker-image go-builder

jvm-test-images:
	@kind load docker-image jvm-env
	@kind load docker-image jvm-builder

nodejs-test-images:
	@kind load docker-image node-env
	@kind load docker-image node-builder

python-test-images:
	@kind load docker-image python-env
	@kind load docker-image python-builder

python-fastapi-test-images:
	@kind load docker-image python-fastapi-env
	@kind load docker-image python-fastapi-builder

router-port-forward:
	@kubectl port-forward svc/router 8888:80 -nfission &
