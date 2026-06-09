# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this repo is

Language runtime environments for [Fission](https://fission.io) (Kubernetes serverless framework).
Each top-level directory (`go/`, `python/`, `nodejs/`, `jvm/`, `binary/`, `dotnet8/`, etc.) is one self-contained environment that produces Docker images published to `ghcr.io/fission`.

Every environment follows the same layout: `server.*` (the runtime HTTP server), `Dockerfile` + `Makefile` (runtime image), optional `builder/` (builder image with `build.sh`), `envconfig.json` (metadata; the `version` field drives releases), and `tests/` or `test/`.

Runnable examples live in the [fission/examples](https://github.com/fission/examples) repo, not here.
Each `tests/` (or `test/`) dir keeps a small `fixtures/` directory with the minimal function code its CI needs.

## Quick commands

```sh
# Local image build (a bare `make` tries to PUSH multi-arch to ghcr.io!)
cd python/ && make DOCKER_FLAGS=--load PLATFORMS=linux/arm64

# Cluster-free unit tests (binary, nodejs, python, python-fastapi)
cd nodejs/ && ./test/local_test.sh

# E2e against a kind cluster (envs with e2e jobs)
SKAFFOLD_PROFILE=python make skaffold-run
make python-test-images router-port-forward
./test_utils/run_test.sh ./python/tests/test_python_env.sh

# After any envconfig.json change (never hand-edit environments.json)
make update-env-json
```

## Detailed guides

Read the relevant file before working in that area:

- [.claude/resources/build-and-images.md](.claude/resources/build-and-images.md) — make/buildx system, where build args live (and drift between), multi-arch rules, local build recipes.
- [.claude/resources/runtime-architecture.md](.claude/resources/runtime-architecture.md) — the specialize protocol (v1/v2), per-language entrypoint semantics, builder contract.
- [.claude/resources/ci.md](.claude/resources/ci.md) — workflow structure, path-filter gotchas, how e2e tests pick images, debugging CI failures.
- [.claude/resources/release-process.md](.claude/resources/release-process.md) — version-bump-driven releases, the GHCR gate, reconcile mode, multi-PR trains.
- [.claude/resources/environment-notes.md](.claude/resources/environment-notes.md) — per-environment quirks and history (jvm's vendored dependency, EOL legacy dotnet, amd64-only tensorflow, etc.).

## Hard rules

- `environments.json` is generated — regenerate with `make update-env-json`, never edit by hand.
- Bumping `version` in any `envconfig.json` triggers an image release when merged to master.
- Build args are duplicated across each env `Makefile`, its `builder/Makefile`, and `skaffold.yaml` — update all three together.
- The fission.io website mirrors `environments.json`; after image renames or new environments, sync the site (see release-process.md).
