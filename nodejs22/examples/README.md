# Fission Node.js 22 Examples

This directory contains examples showcasing **Node.js 22** with **Pure ESM** support and modern JavaScript features.

🚀 **What's New in Node.js 22:**
- **Native ESM Support** - Use `import`/`export` syntax
- **Top-level await** - No need to wrap in async functions
- **Latest JavaScript features** - Modern syntax and APIs
- **Enhanced performance** - Faster execution and better memory usage

## Environment Setup

Create a Node.js 22 environment with ESM support:

```bash
# Option 1: Use our pre-built Node.js 22 environment
$ fission env create --name node22 --image davidchase03/node-env-22:latest

# Option 2: Use official images (when available)
$ fission env create --name node22 --image fission/node-env-22:latest --builder fission/node-builder-22:latest
```

## Function Signatures

### 🎯 **Pure ESM (Node.js 22 Standard)**
All examples use modern ESM syntax:
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
export default entry1; // default export for backward compatibility
```

## 📋 **Examples Overview**

| Example | Description | Node.js 22 Features |
|---------|-------------|---------------------|
| `hello.js` | ESM hello world | Pure ESM module |
| `weather.js` | HTTP requests demo | Native fetch API, error handling |
| `multi-entry.js` | Multiple endpoints | Named exports, routing |
| `broadcast.js` | WebSocket broadcasting | Multi-client messaging |
| `kubeEventsSlack.js` | Kubernetes integration | Event processing, webhooks |

### **📦 Pure ESM Environment:**
- **All `.js` files** use ESM syntax (`import`/`export`)
- **Native Node.js 22** ESM support with `"type": "module"` 
- **For CommonJS examples**, see the regular `nodejs` environment folder

## 🚀 **Quick Start**

1. **Create the environment:**
```bash
fission env create --name node22 --image davidchase03/node-env-22:latest
```

2. **Deploy a function:**
```bash
fission fn create --name hello --env node22 --code hello.cjs
```

3. **Test it:**
```bash
fission fn test --name hello
```

4. **Create an HTTP route:**
```bash
fission route create --method GET --url /hello --function hello
curl $FISSION_ROUTER/hello
```

---

## 📖 **Individual Examples**


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

## 🛠️ **Development Tips**

### **Node.js 22 Best Practices:**
1. **Use ESM when possible** - Future-proof your functions
2. **Leverage optional chaining** - Safer property access
3. **Use Promise.all()** - Parallel async operations
4. **Monitor performance** - Built-in performance APIs
5. **Handle errors gracefully** - Promise.allSettled() for mixed results

### **Fission-Specific Tips:**
1. **Keep functions small** - Better cold start performance
2. **Use CommonJS for compatibility** - ESM support is evolving
3. **Return proper HTTP responses** - Include status, body, headers
4. **Test locally first** - Use the test runner before deployment

### **Debugging:**
```bash
# View function logs
fission fn logs --name your-function

# Check environment status
fission env list

# Monitor function performance
fission fn test --name your-function --verbose
```

---

## 🔗 **Additional Resources**

- [Node.js 22 Documentation](https://nodejs.org/docs/latest-v22.x/)
- [Fission Documentation](https://fission.io/docs/)
- [JavaScript Modern Features](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
- [ESM in Node.js](https://nodejs.org/api/esm.html)

---

*Happy serverless coding with Node.js 22! 🚀*
