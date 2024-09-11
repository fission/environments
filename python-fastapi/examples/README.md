# Python Examples

This directory contains a Python examples to show different the features of the Fission Python environment:
- `hello.py` is a simple Pythonic _hello world_ function.

## Getting Started

Create a Fission Python environment with the default Python runtime image (this does not include the build environment):
```
fission environment create --name python --image ghcr.io/fission/python-fastapi-env
```

Use the `hello.py` to create a Fission Python function:
```
fission function create --name hello-py --env python --code hello.py 
```

Test the function:
```
fission function test --name hello-py
```
