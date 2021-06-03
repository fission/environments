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

.DEFAULT_GOAL := multiarch-all

# Platforms to build in multi-architecture images.
PLATFORMS ?= linux/amd64,linux/arm64,linux/arm/v7

# Repository prefix and tag to push multi-architecture images to.
REPO ?= fission
TAG ?= dev
PUSH ?= --push 

verify-builder: 
	@./hack/buildx.sh 

multiarch-all: multiarch-binary multiarch-go multiarch-python multiarch-nodejs multiarch-perl multiarch-php multiarch-ruby

multiarch-binary: verify-builder multiarch-binary-env multiarch-binary-builder

multiarch-go: verify-builder multiarch-go-env multiarch-go-builder

multiarch-python: verify-builder multiarch-python-env multiarch-python-builder

multiarch-nodejs: verify-builder multiarch-nodejs-env multiarch-nodejs-builder

multiarch-perl: verify-builder multiarch-perl-env 

multiarch-php: verify-builder multiarch-php-env  multiarch-php-builder

multiarch-ruby: verify-builder multiarch-ruby-env multiarch-ruby-builder

multiarch-binary-env: 
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/binary-env:$(TAG) $(PUSH) -f binary/Dockerfile binary/

multiarch-binary-builder:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/binary-builder:$(TAG) $(PUSH) -f  binary/builder/Dockerfile binary/builder/

multiarch-go-env:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/go-env:$(TAG) $(PUSH) -f go/Dockerfile go/

multiarch-go-builder:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/go-builder:$(TAG) $(PUSH) -f  go/builder/Dockerfile go/builder/

multiarch-python-env:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/python-env:$(TAG) $(PUSH) -f python/Dockerfile python/

multiarch-python-builder:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/python-builder:$(TAG) $(PUSH) -f python/builder/Dockerfile python/builder

multiarch-nodejs-env:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/nodejs-env:$(TAG) $(PUSH) -f nodejs/Dockerfile nodejs/

multiarch-nodejs-builder:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/nodejs-builder:$(TAG) $(PUSH) -f nodejs/builder/Dockerfile nodejs/builder/

multiarch-perl-env:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/perl-env:$(TAG) $(PUSH) -f perl/Dockerfile perl/

multiarch-php-env:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/php-env:$(TAG) $(PUSH) -f php7/Dockerfile php7/

multiarch-php-builder:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/php-builder:$(TAG) $(PUSH) -f php7/builder/Dockerfile php7/builder/

multiarch-ruby-env:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/ruby-env:$(TAG) $(PUSH) -f ruby/Dockerfile ruby/

multiarch-ruby-builder:
	docker buildx build --platform=$(PLATFORMS) -t $(REPO)/ruby-builder:$(TAG) $(PUSH) -f ruby/builder/Dockerfile ruby/builder/ 