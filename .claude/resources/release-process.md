# Release process

## Version-bump-driven releases

1. Bump `version` in `<env>/envconfig.json` (this is the image tag to publish).
2. Run `make update-env-json` — sorts every `envconfig.json` (jq) and regenerates the root `environments.json`.
   Never hand-edit `environments.json`; commit the regenerated file with the bump.
3. On merge to master, `.github/workflows/release.yaml` (path filter `**/envconfig.json**`) runs `hack/release_check.py`, which emits a matrix of every `image:version` not yet on ghcr.io; the workflow's `docker-buildx-push` job then runs `TAG=<version> make <image>-img` (and `<builder>-img` in `builder/`) plus a `latest` push for each matrix entry.

Image content changes without an envconfig bump never release — if a merged change should reach the published image (e.g. a lockfile refresh), follow up with a version-bump PR.
Conversely, examples/ and docs changes don't need a bump (they're not in the image).

## release_check.py semantics

- Checks GHCR via the v2 API with an anonymous bearer token.
- Token endpoint 401/403/404 ⇒ the package doesn't exist yet ⇒ release needed (GHCR refuses tokens for unknown packages — this is the first-release path for renamed/new images).
- tags/list 200 ⇒ skip if tag present; 404 ⇒ release; anything else raises (fail-closed so registry hiccups can't trigger mass re-pushes of `latest`).
- Outputs go to `$GITHUB_OUTPUT`; `release_needed` is lowercase `true`/`false` and release.yaml gates on `== 'true'` — keep these in sync.
- **Reconcile mode**: invoked with no package list (e.g. `gh workflow run release.yaml`), it scans every `*/envconfig.json` and releases anything unpublished.
  Use this to backfill after a failed release run.
  Expect the legacy `dotnet`/`dotnet20` matrix legs to fail (EOL bases); `fail-fast: false` keeps other legs going.
- Testable locally: `GITHUB_OUTPUT= python3 hack/release_check.py '[python,go]'` (needs `requests`).

## Multi-PR trains

Every env PR rewrites the generated `environments.json`, so PRs in a series conflict with each other on that file.
Merge serially; for each next PR: `git merge origin/master`, run `make update-env-json` to resolve the conflict canonically, `git add environments.json`, commit, push, wait for green, merge.
Take master's side for workflow-file conflicts when master's change is a superset of the branch's.

## Version pin locations for the Fission version

The Fission version string must be bumped together in four places (note the differing key names — grepping for `FISSION_VERSION` alone misses two):
`FISSION_VERSION` in `rules.mk` and in `environment.yaml`'s `env`, the `fission-cli-version` input default in `setup-cluster/action.yml`, and the hardcoded skaffold `remoteChart` URL (chart tags have no `v` prefix: `fission-all-1.25.0`).

## Downstream: fission.io website

The site mirrors this repo's catalog.
After image renames, new environments, or removals, sync the site repo (it has a `updating-environments-and-examples` skill):

- `static/data/environments.json` stores image/builder *names* only (not versions) — only name changes matter there.
- `tools/environments.py` regenerates it from this repo's manifest and is keyed by image name (display names are not unique — both jvm and jvm-jersey report "JVM Environment").
- Docs pages may embed versioned image names in examples (grep for `go-env-1.`, old runtime versions); leave historical release-notes pages untouched.
