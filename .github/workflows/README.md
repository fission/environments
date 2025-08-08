# GitHub Actions Workflow for Fission Environments

This directory contains the GitHub Actions workflow configuration for building and testing Fission environments.

## Structure

- `environment.yaml`: Main workflow file that defines jobs for all environments
- `actions/`: Directory containing reusable composite actions
  - `setup-cluster/`: Sets up Helm, Kind cluster, Fission CLI and runs the base Fission setup
  - `collect-fission-dump/`: Collects and archives Fission dumps
- `filters/`: Contains path filters for detecting changes

## Composite Actions

The workflow uses two main composite actions to reduce duplication:

1. **setup-cluster**: Sets up the complete infrastructure needed for testing (Helm, Kind, Fission CLI) and performs base Fission setup steps
2. **collect-fission-dump**: Collects and archives Fission dumps on failure

## Environment Variables

All version pins are centralized in the `env` section of the main workflow file:

- `KIND_NODE_IMAGE`: Kind node image version
- `KIND_VERSION`: Kind tool version
- `HELM_VERSION`: Helm version
- `FISSION_CLI_VERSION`: Fission CLI version
- `KIND_CONFIG`: Path to Kind configuration
- `FISSION_VERSION`: Fission version

## Usage

The workflow is triggered on pull requests to the `master` branch. It first runs a change detection job to determine which environments have been modified, then runs the relevant jobs for those environments.

### Adding a New Environment

To add a new environment:

1. Add the environment to the `check` job's outputs
2. Create a new job for the environment following the existing patterns
3. Add the environment to the filters configuration
