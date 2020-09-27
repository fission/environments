# Rust Environment

This is the Rust environment for Fission.

The builder in this environment builds the cargo project. In the function container, the compiled binary will be called by a Go server.

For examples please visit the [Rust examples directory](examples)


⚠️ **Words of Caution** ⚠️

The environment runs on an alpine image. Alpine uses the musl C standard library. You should try building your project with this target. You could change the image e.g. to `debian` (slim), see [Compiling](#Compiling) 


## Usage
To get started with the latest rust environment:

```bash
fission env create --name rust --image fission/rust-env --builder fission/rust-builder
```
After this, fission functions that have the env parameter set to the
same environment name as this command will use this environment.

## Compiling

To build the runtime environment:
```bash
docker build --tag=${USER}/rust-env .
```

To build the builder environment:
```bash
(cd builder/ && docker build --tag=${USER}/rust-builder .)
```
