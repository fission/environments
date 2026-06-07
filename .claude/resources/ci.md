# CI

## Workflow structure

`.github/workflows/environment.yaml` runs on PRs to master.
A `check` job runs `dorny/paths-filter` (filters in `.github/workflows/filters/filters.yaml`); each env job gates on its filter key, so only changed environments build and test.

Two kinds of env jobs:

- **Full e2e** (binary, go, jvm, nodejs, python, python-fastapi, dotnet8): setup-cluster → `SKAFFOLD_PROFILE=<env> make skaffold-run` → `make <env>-test-images` (kind-load) → `make router-port-forward` → `./test_utils/run_test.sh <env>/tests/test_*_env.sh` → fission dump on failure.
- **Build-only** (perl, php7, ruby, tensorflow, jvm-jersey): setup-cluster + `make skaffold-run` only; no functional test.
  Compensate with local container smoke tests (specialize + invoke) before pushing changes to these envs.

Composite actions: `.github/actions/setup-cluster` (helm + `helm/kind-action` with `cluster_name: kind` + fission CLI + skaffold install + crds; version pins live in its input defaults) and `.github/actions/collect-fission-dump` (best-effort by design — must never mask the original failure).

## Gotchas (each of these caused a real failure)

1. **E2e tests must pin the local image.**
   Test scripts must `export <ENV>_RUNTIME_IMAGE=<image>` to the kind-loaded name (e.g. `jvm-env`, `go-env`).
   The fallback defaults in `test_utils/utils.sh` point at years-stale Docker Hub images (`fission/jvm-env` etc.) and silently test the wrong image.
2. **Workflow-only PRs exercise nothing.**
   A PR touching only `.github/` triggers no env jobs, so composite-action changes go unvalidated and can break master for every subsequent run.
   Include a small genuine change under one env dir (e.g. a `perl/` README fix) to force one job through the changed path before merging.
3. **Exact-match the filter gates.**
   `packages` is a JSON array string; use quoted matches like `contains(needs.check.outputs.packages, '"jvm"')`.
   Bare substrings cross-trigger: `jvm` matches `jvm-jersey`, `python` matches `python-fastapi`.
   Also: it's `needs.check.outputs.packages` — `needs.check.outputs` alone never matches (historical bug that kept the python job from ever running).
4. **Don't reintroduce action pins that ship without compiled dist.**
   `engineerd/setup-kind@v0.6.2` failed with `File not found: dist/main/index.js`; `helm/kind-action` is the maintained replacement.
   `hiberbee/github-action-skaffold` pins skaffold 2.3.1 which cannot parse `skaffold/v4beta13` — use `make skaffold-run` instead.

## Test harness

- `test_utils/run_test.sh [files...]` runs tests via GNU parallel and aggregates logs; a file containing the line `#test:disabled` is skipped.
- macOS prerequisites: `brew install coreutils findutils gnu-sed parallel` (see `test_utils/init_tools.sh`).
- Some envs have cluster-free `local_test.sh` (binary, nodejs, python, python-fastapi) — run these first, they catch dependency breakage in seconds.

## Debugging CI

- `gh run view <id> --log-failed` for failing steps; e2e test output is embedded in the `run_test.sh` log dump.
- Function-level failures need the fission dump artifact (`<env>-fission-dump`); a `test_fn` curl loop timing out (exit 124) usually means the function pod never became ready — check the env image actually used (gotcha 1).
- Local e2e reproduction works with kind + skaffold + fission CLI installed (`make verify-kind-cluster create-crds`, then the same steps as CI).
