# Fission Node.js 22 Examples

This directory contains examples showcasing **Node.js 22** with **dual ESM/CJS** support and modern JavaScript features.

**What's New in Node.js 22:**
- **Dual Module Support** - Both ESM (`import`/`export`) and CJS (`require`/`module.exports`)
- **Native ESM Support** - Use `import`/`export` syntax as default
- **Top-level await** - No need to wrap in async functions
- **Latest JavaScript features** - Modern syntax and APIs
- **Enhanced performance** - Faster execution and better memory usage

## LOAD_ESM Environment Variable Control

The `LOAD_ESM` environment variable controls how `.js` files are interpreted:
- **LOAD_ESM=true (Default)**: `.js` files use ESM (`import`/`export`)
- **LOAD_ESM=false**: `.js` files use CJS (`require()`/`module.exports`)

## Environment Setup

### **ESM Environment (Default):**
```bash
fission env create --name node22-esm \
  --image davidchase03/node-env-22:v3.0.0 \
  --builder davidchase03/node-builder-22:v3.0.0 \
  --runtime-env LOAD_ESM=true
```

### **CJS Environment:**
```bash
fission env create --name node22-cjs \
  --image davidchase03/node-env-22:v3.0.0 \
  --builder davidchase03/node-builder-22:v3.0.0 \
  --runtime-env LOAD_ESM=false
```


## Function Signatures

### **ESM (ES Modules - Default)**
ESM examples use modern ES6+ syntax:
```javascript
// Single function export
export default async function(context) {
    return {
        status: 200,
        body: JSON.stringify({
            message: "Hello from Node.js 22 ESM! 🚀",
            nodeVersion: process.version
        }),
        headers: {
            'Content-Type': 'application/json'
        }
    }
}
```

```javascript
// Named exports for multiple entry points
export const entry1 = async (context) => { /* ... */ };
export const entry2 = async (context) => { /* ... */ };
```

### **CJS (CommonJS - Traditional)**
CJS examples use traditional Node.js syntax with `.cjs` extension:
```javascript
// Single function export
module.exports = async function(context) {
    return {
        status: 200,
        body: JSON.stringify({
            message: "Hello from Node.js 22 CJS!",
            nodeVersion: process.version
        }),
        headers: {
            'Content-Type': 'application/json'
        }
    }
}

// Callback pattern
module.exports = function(context, callback) {
    callback(200, JSON.stringify({ message: "Hello!" }), {
        'Content-Type': 'application/json'
    });
};
```

## Examples Overview

### ESM Examples (ES Modules)
| Example | Description | Features |
|---------|-------------|----------|
| `hello.js` | ESM hello world | Default export, modern syntax |
| `weather.js` | HTTP requests demo | Native fetch API, error handling |
| `multi-entry.js` | Multiple endpoints | Named exports, routing |
| `broadcast.js` | WebSocket broadcasting | Multi-client messaging |
| `kubeEventsSlack.js` | Kubernetes integration | Event processing, webhooks |

### CJS Examples for LOAD_ESM=false (CommonJS Mode)
| Example | Description | Features |
|---------|-------------|----------|
| `hello-cjs.js` | CJS hello world with .js | Basic module.exports with .js extension |
| `callback-cjs.js` | Callback pattern with .js | Traditional callback style with .js extension |
| `require-cjs.js` | Using require() with .js | Node.js built-ins with require() and .js extension |

### Legacy CJS Example (works in both modes)
| Example | Description | Features |
|---------|-------------|----------|
| `index.cjs` | CJS hello world | Basic module.exports with .cjs extension (legacy compatibility) |

### LOAD_ESM Environment Variable:
- **LOAD_ESM=true (Default)**: `.js` files use ESM (`import`/`export`), `.cjs` files use CJS
- **LOAD_ESM=false**: `.js` files use CJS (`require()`/`module.exports`), `.cjs` files use CJS  
- **Always**: `.mjs` files always use ESM, `.cjs` files always use CJS

## Quick Start

### **ESM Quick Start (Default):**
1. **Create ESM environment:**
```bash
fission env create --name node22-esm \
  --image davidchase03/node-env-22:v3.0.0 \
  --builder davidchase03/node-builder-22:v3.0.0 \
  --runtime-env LOAD_ESM=true
```

2. **Deploy ESM function:**
```bash
fission fn create --name hello-esm --env node22-esm --code hello.js
```

3. **Test it:**
```bash
fission fn test --name hello-esm
```

### **CJS Quick Start:**
1. **Create CJS environment:**
```bash
fission env create --name node22-cjs \
  --image davidchase03/node-env-22:v3.0.0 \
  --builder davidchase03/node-builder-22:v3.0.0 \
  --runtime-env LOAD_ESM=false
```

2. **Deploy CJS function:**
```bash
fission fn create --name hello-cjs --env node22-cjs --code hello-cjs.js
```

3. **Test it:**
```bash
fission fn test --name hello-cjs
```

### **Create HTTP routes:**
```bash
fission route create --method GET --url /hello-esm --function hello-esm
fission route create --method GET --url /hello-cjs --function hello-cjs

# Test via HTTP (requires port forwarding)
kubectl port-forward -n fission svc/router 8888:80 &
curl http://localhost:8888/hello-esm
curl http://localhost:8888/hello-cjs
```

---

## Individual Examples


### `hello.js` - ESM Hello World  
Modern ESM version with query parameter support.

```bash
fission fn create --name hello-esm --env node22 --code hello.js
fission fn test --name hello-esm
fission route create --method GET --url /hello-esm --function hello-esm
curl "$FISSION_ROUTER/hello-esm?name=World"
```


### `weather.js` - HTTP Requests
External API integration example using modern fetch patterns.

```bash
fission fn create --name weather --env node22 --code weather.js
fission fn test --name weather --body '{"location":"New York"}'
```

---

### Debugging:
```bash
# View function logs
fission fn logs --name your-function

# Check environment status
fission env list

# Monitor function performance
fission fn test --name your-function --verbose
```

---

## Additional Resources

- [Node.js 22 Documentation](https://nodejs.org/docs/latest-v22.x/)
- [Fission Documentation](https://fission.io/docs/)
- [ESM in Node.js](https://nodejs.org/api/esm.html)

---

