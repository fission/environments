# Rust Environment Examples

These are examples for the Fission Rust environment.

For more info read the [environment README](../README.md).

## Requirements

First, set up your fission deployment with the rust environment.

```bash
fission env create --name rust --image fission/rust-env --builder fission/rust-builder
```

## Example Usage

### hello-world
`hello-world` is a minimal cargo project that returns `"Hello, World!"` and prints all current environment variables.

```bash
# Upload the function to fission
fission function create --name hello-rust --env rust --src hello-world/

# Test the function
fission function test --name hello-rust

# Map /hello-rust to the hello-rust function
fission route create --method GET --url /hello-rust --function hello-rust

# Run the function
curl -H 'X-RUST: MEMORYSAFE!' http://$FISSION_ROUTER/hello-rust
```

This should return a HTTP response with the body `Hello World!` and the environment variables inside the runtime container.

Currently the whole content of the folder `hello-world` will be copied. So make sure that the folder only contains the files needed for compilation.
