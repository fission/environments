// CJS multiple exports test case using .js extension for LOAD_ESM=false
const crypto = require('crypto');

function generateId() {
  return crypto.randomBytes(4).toString('hex');
}

async function mainHandler(context) {
  return {
    status: 200,
    body: JSON.stringify({
      message: "CJS Test Case 3 - .js extension multiple exports",
      moduleType: "CJS",
      pattern: "multi-export",
      fileExtension: ".js",
      loadMode: process.env.LOAD_ESM !== 'false' ? 'ESM' : 'CJS',
      id: generateId(),
      testCase: "js-cjs-multi-export",
      timestamp: new Date().toISOString()
    }, null, 2),
    headers: {
      "Content-Type": "application/json"
    }
  };
}

function alternateHandler(context) {
  return {
    status: 200,
    body: JSON.stringify({
      message: "Alternate CJS Handler with .js extension",
      moduleType: "CJS",
      handler: "alternate",
      fileExtension: ".js",
      loadMode: process.env.LOAD_ESM !== 'false' ? 'ESM' : 'CJS',
      timestamp: new Date().toISOString()
    }, null, 2),
    headers: {
      "Content-Type": "application/json"
    }
  };
}

// Export both functions
module.exports = mainHandler;
module.exports.mainHandler = mainHandler;
module.exports.alternateHandler = alternateHandler;