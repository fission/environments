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