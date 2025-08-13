// CommonJS version using .js extension  
// Use with LOAD_ESM=false environment
module.exports = async function(context) {
  return {
    status: 200,
    body: JSON.stringify({
      message: "Hello from Node.js 22 CommonJS with .js extension!",
      nodeVersion: process.version,
      moduleType: "CJS",
      fileExtension: ".js", 
      loadMode: process.env.LOAD_ESM !== 'false' ? 'ESM' : 'CJS',
      timestamp: new Date().toISOString()
    }, null, 2),
    headers: {
      "Content-Type": "application/json"
    }
  };
};