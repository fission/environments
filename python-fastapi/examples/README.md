# Python Examples

This directory contains a Python examples to show different the features of the Fission Python environment:
- `hello.py` is a simple Pythonic _hello world_ function.
- `sourcepkg/` is an example of how to use the Fission Python Build environment to resolve (pip) dependencies
  before deploying the function.
- `multifile/` shows how to create Fission Python functions with multiple source files.
- `requestdata.py` shows how you can access the HTTP request fields, such as the body, headers and query.
- `statuscode.py` is an example of how you can change the response status code.
- `guestbook/` is a more realistic demonstration of using Python and Fission to create a serverless guestbook.

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
