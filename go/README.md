# Fission: Go Environment

This is the Go environment for Fission.

It's a Docker image containing a Go runtime, along with a dynamic loader.

Looking for ready-to-run examples? See the [Go examples directory](../../examples/go).

## Build this image

```sh
docker build -t USER/go-env --build-arg GO_VERSION=1.16 -f Dockerfile-1.1x . && docker push USER/go-env
```

Note that if you build the runtime, you must also build the go-builder
image, to ensure that it's at the same version of go:

```sh
cd builder && docker build -t USER/go-builder --build-arg GO_VERSION=1.16 -f Dockerfile-1.1x . && docker push USER/go-builder
```

## Using the image in fission

You can add this customized image to fission with "fission env
create":

```sh
fission env create --name go --image USER/go-env --builder USER/go-builder --version 2
```

Or, if you already have an environment, you can update its image:

```sh
fission env update --name go --image USER/go-env --builder USER/go-builder
```

After this, fission functions that have the env parameter set to the
same environment name as this command will use this environment.
