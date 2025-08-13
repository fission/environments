// CommonJS with require() using .js extension
// Use with LOAD_ESM=false environment
const fs = require('fs');
const path = require('path');

module.exports = async function(context) {
  try {
    const operation = context.request.query.op || 'status';
    
    let result = {
      operation,
      moduleType: "CJS",
      fileExtension: ".js",
      loadMode: process.env.LOAD_ESM !== 'false' ? 'ESM' : 'CJS',
      nodeVersion: process.version,
      timestamp: new Date().toISOString()
    };
    
    switch (operation) {
      case 'env':
        result.environment = {
          nodeEnv: process.env.NODE_ENV || 'not-set',
          platform: process.platform,
          arch: process.arch,
          cwd: process.cwd(),
          loadEsm: process.env.LOAD_ESM
        };
        break;
        
      case 'status':
      default:
        result.status = 'CommonJS function with require() working correctly';
        result.features = [
          'require() syntax',
          'module.exports',
          'async/await support',
          '.js extension in CJS mode'
        ];
    }
    
    return {
      status: 200,
      body: JSON.stringify(result, null, 2),
      headers: {
        "Content-Type": "application/json"
      }
    };
    
  } catch (error) {
    return {
      status: 500,
      body: JSON.stringify({
        error: error.message,
        moduleType: "CJS",
        fileExtension: ".js",
        timestamp: new Date().toISOString()
      }),
      headers: {
        "Content-Type": "application/json"
      }
    };
  }
};