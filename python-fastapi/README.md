# Fission: Python FastAPI Environment

This is the Python environment for Fission based on FastAPI framework.

It's a Docker image containing a Python 3.11 runtime, along with a
dynamic loader.  A few common dependencies are included in the
requirements.txt file.

Looking for ready-to-run examples? See the [Python FastAPI examples directory](./examples).

## Customizing this image

To add package dependencies, edit requirements.txt to add what you
need, and rebuild this image (instructions below).

You also may want to customize what's available to the function in its
request context.  You can do this by editing server.py (see the
comment in that file about customizing request context).

## Rebuilding and pushing the image

You'll need access to a Docker registry to push the image: you can
sign up for Docker hub at hub.docker.com, or use registries from
gcr.io, quay.io, etc.  Let's assume you're using a docker hub account
called USER.  Build and push the image to the the registry:

```
   docker build -t USER/python-fastapi-env --build-arg PY_BASE_IMG=3.11-alpine . && docker push USER/python-fastapi-env
```

## Using the image in fission

You can add this customized image to fission with "fission env create":

```
   fission env create --name python --image USER/python-fastapi-env
```

Or, if you already have an environment, you can update its image:

```
   fission env update --name python --image USER/python-fastapi-env
```

After this, fission functions that have the env parameter set to the
same environment name as this command will use this environment.

## Web Server Framework

Python environment build and start a ASGI server, to support high HTTP
traffic. It provides Uvicorn server framework.
