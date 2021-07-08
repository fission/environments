# MULTI-ARCH COMPATIBILITY

The following environments have been tested as multi-architecture builds as well as on amd64:

* Go     : confirmed working (1) on arm, arm64.
* Nodejs : confirmed working on arm, arm64.
* Perl   : confirmed working on arm, arm64.
* PHP 7  : confirmed working on arm, arm64.
* Python : confirmed working on arm, arm64.
* Ruby   : confirmed working (2) on arm, arm64.

1. Only when function is supplied in a package; single-file functions do not work.
2. Requires entrypoint to be explicitly specified, even on single-file functions.
