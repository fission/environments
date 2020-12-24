# MULTI-ARCH COMPATIBILITY

The following environments have been tested as multi-architecture builds as well as on amd64:

* Binary: confirmed working on arm, arm64 (1).
* Nodejs: confirmed working on arm, arm64.
* Python: confirmed working on arm, arm64.

1. Note: only suitable for shell scripts if the cluster contains nodes of multiple architectures; at present, Fission has no way to restrict execution of a function to solely nodes/containers of one particular architecture.

