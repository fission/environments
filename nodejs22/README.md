# Fission: NodeJS 22 Environment

This is the NodeJS 22 environment for Fission with full support for both **ESM (ES Modules)** and **CJS (CommonJS)** function patterns.

It's a Docker image containing a NodeJS runtime, along with a dynamic
loader that can handle both modern ES modules and traditional CommonJS modules.  A few common dependencies are included in the package.json file.

## Module System Support

This environment supports both module systems:

- **ESM (Default)**: Modern ES modules with `import`/`export` syntax (`.js`, `.mjs` files)
- **CJS**: CommonJS modules with `require()`/`module.exports` syntax (`.cjs` files)

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

## Function Patterns

The nodejs22 environment supports both ESM and CJS through the `LOAD_ESM` environment variable:

### ESM Mode (LOAD_ESM=true - default)

In ESM mode, `.js` files are treated as ES Modules:

```javascript
// hello.js - ESM default export
export default async (context) => {
  return {
    status: 200,
    body: JSON.stringify({ message: "Hello from ESM!" }),
    headers: { "Content-Type": "application/json" }
  };
};

// Or using named exports
export async function myHandler(context) {
  // function implementation
}
```

### CJS Mode (LOAD_ESM=false)

In CJS mode, `.js` files are treated as CommonJS modules:

```javascript
// hello.js - CJS async function (when LOAD_ESM=false)
module.exports = async function(context) {
  return {
    status: 200,
    body: JSON.stringify({ message: "Hello from CJS!" }),
    headers: { "Content-Type": "application/json" }
  };
};

// hello.js - CJS callback pattern (when LOAD_ESM=false)
module.exports = function(context, callback) {
  callback(200, JSON.stringify({ message: "Hello from CJS callback!" }), {
    "Content-Type": "application/json"
  });
};
```

### File Extension Behavior

| LOAD_ESM | .js files | .mjs files | .cjs files |
|----------|-----------|------------|------------|
| true     | ESM       | ESM        | CJS        |
| false    | CJS       | ESM        | CJS        |

### Function Discovery

The environment automatically detects and loads functions based on:

1. **File Extension Priority**: `.js` (ESM), `.mjs` (ESM), `.cjs` (CJS)
2. **Entry Point**: `index.*`, `main.*`, or specified in `package.json`
3. **Function Names**: Support for both default exports and named functions

### Using the LOAD_ESM Environment Variable

The `LOAD_ESM` environment variable controls how `.js` files are interpreted:

- **LOAD_ESM=true (default)**: `.js` files use ESM syntax (`import`/`export`)
- **LOAD_ESM=false**: `.js` files use CJS syntax (`require()`/`module.exports`)

You can set this at the environment level or per function:

### Environment Setup

**ESM Environment (default):**
```bash
fission env create --name node22-esm \
  --image fission/node-env-22 \
  --builder fission/node-builder-22
```

**CJS Environment:**
```bash
fission env create --name node22-cjs \
  --image fission/node-env-22 \
  --builder fission/node-builder-22 \
  --runtime-env LOAD_ESM=false
```

**Per-Function Override:**
```bash
# Create a CJS function in an ESM environment
fission fn create --name my-cjs-func \
  --env node22-esm \
  --code hello-cjs.js \
  --runtime-env LOAD_ESM=false

# Create an ESM function in a CJS environment  
fission fn create --name my-esm-func \
  --env node22-cjs \
  --code hello.js \
  --runtime-env LOAD_ESM=true
```

### Examples Available

**ESM Examples (use with ESM environment):**
- `hello.js` - ESM default export  
- `weather.js` - ESM with HTTP requests
- `multi-entry.js` - ESM with named exports

**CJS Examples (use with CJS environment):**
- `hello-cjs.js` - CJS basic function with `.js` extension
- `callback-cjs.js` - CJS callback pattern with `.js` extension  
- `require-cjs.js` - CJS with require() using `.js` extension

**Legacy CJS Example (works in both environments):**
- `index.cjs` - CJS basic function with `.cjs` extension (shows legacy compatibility)
