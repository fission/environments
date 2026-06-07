# Build system and images

## rules.mk

Every env `Makefile` includes the root `rules.mk` (builders include it via `../../rules.mk`).
Defaults to know:

- `PLATFORMS ?= linux/amd64,linux/arm64` — multi-arch by default.
- `REPO ?= ghcr.io/fission`, `TAG ?= dev`.
- **`DOCKER_FLAGS ?= --push`** — a bare `make` attempts to push to ghcr.io.
- The generic rule is `%-img:` → `docker buildx build $($@-buildargs) --platform=$(PLATFORMS) -t $(REPO)/<name>:$(TAG) $(DOCKER_FLAGS) -f $< .`
- Per-target build args are declared as `<image>-img-buildargs := --build-arg KEY=value`.
- Target-specific platform overrides are supported, e.g. `tensorflow-serving-env-img: PLATFORMS=linux/amd64` (the upstream `tensorflow/serving` image is amd64-only).

## Local builds

```sh
cd <env>/ && make <image>-img DOCKER_FLAGS=--load PLATFORMS=linux/arm64
cd <env>/builder/ && make <builder>-img DOCKER_FLAGS=--load PLATFORMS=linux/arm64
```

Use a single platform with `--load`; buildx cannot `--load` a multi-arch manifest.
On Apple Silicon use `linux/arm64` for speed; CI builds run on amd64.
All base images used are multi-arch except `tensorflow/serving`.

## Where build args live (three places, keep in sync)

The same base-image pin is duplicated in:

1. `<env>/Makefile` (`<image>-img-buildargs`)
2. `<env>/builder/Makefile` (`<builder>-img-buildargs`) — easy to miss; it has bitten before
3. `skaffold.yaml` (the env's build profile `buildArgs`, used by CI)

Dockerfile `ARG` defaults are a fourth copy in some envs (python).
When bumping a base image, grep for the old value across all of these plus READMEs and `envconfig.json`'s `runtimeVersion`.

## Build contexts

- The env image context is `<env>/`; the builder image context is `<env>/builder/`.
- A file needed by both images (e.g. `jvm/install-fission-java-core.sh`) must be physically duplicated into `builder/` — symlinks break because Docker contexts don't follow them out of tree.
  Mark such copies with a keep-in-sync header comment.

## skaffold.yaml

- Per-env build profiles plus a helm deploy of the fission chart (`remoteChart` URL pins the fission version; chart release tags have no `v` prefix, e.g. `fission-all-1.25.0`).
- CI uses `SKAFFOLD_PROFILE=<env> make skaffold-run`; skaffold with the kind profile loads built images into the kind cluster.
- Some skaffold image names differ from release image names (e.g. profile builds `jvm-jersey-env` while releases publish `jvm-jersey-env-25`).
