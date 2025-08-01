# Fission: NodeJS 22 Environment

This is the NodeJS 22 environment for Fission.

It's a Docker image containing a NodeJS runtime, along with a dynamic
loader.  A few common dependencies are included in the package.json
file.

Looking for ready-to-run examples? See the [NodeJS examples directory](./examples/).

## Customizing this image

To add package dependencies, edit [package.json](./package.json) to add what you need, and rebuild this image (instructions below).

You also may want to customize what's available to the function in its request context.
You can do this by editing [server.js](./server.js) (see the comment in that file about customizing request context).

## Rebuilding and pushing the image

You'll need access to a Docker registry to push the image: you can sign up for Docker hub at hub.docker.com, or use registries from gcr.io, quay.io, etc.
Let's assume you're using a docker hub account called USER.
Build and push the image to the the registry:

Building runtime image,

```console
docker build -t USER/node-env-22 --build-arg NODE_BASE_IMG=22.17.1-alpine3.22 -f Dockerfile .
docker push USER/node-env-22
```

Building builder image,

```console
cd builder && docker build -t USER/node-builder-22 --build-arg NODE_BASE_IMG=22.17.1-alpine3.22 -f Dockerfile .
docker push USER/node-builder-22
```

## Using the image in fission

You can add this customized image to fission with "fission env create":

```console
fission env create --name node22 --image USER/node-env-22
```

Or, if you already have an environment, you can update its image:

```console
fission env update --name node22 --image USER/node-env-22
```

After this, fission functions that have the env parameter set to the
same environment name as this command will use this environment.
