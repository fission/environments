// CJS test case using .js extension for LOAD_ESM=false
module.exports = async function(context) {
  return {
    status: 200,
    body: JSON.stringify({
      message: "CJS Test Case 1 - .js extension in CJS mode",
      moduleType: "CJS",
      fileExtension: ".js",
      loadMode: process.env.LOAD_ESM !== 'false' ? 'ESM' : 'CJS',
      testCase: "js-cjs-basic",
      nodeVersion: process.version,
      timestamp: new Date().toISOString()
    }, null, 2),
    headers: {
      "Content-Type": "application/json"
    }
  };
};