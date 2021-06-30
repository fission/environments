# MULTI-ARCH COMPATIBILITY

The following environments have been tested as multi-architecture builds as well as on amd64:

* Binary: confirmed working on arm, arm64 (3).
* Nodejs: confirmed working on arm, arm64.
* Python: confirmed working on arm, arm64.

3. Note: if the cluster contains nodes of multiple architectures, you must create multiple environments limited using PodSpec and nodeSelectors to set the node architectures which particular binary functions execute on. Otherwise only shell scripts will be usable.

