// CJS callback test case using .js extension for LOAD_ESM=false
module.exports = function(context, callback) {
  const delay = parseInt(context.request.query.delay) || 0;
  
  setTimeout(() => {
    callback(200, JSON.stringify({
      message: "CJS Test Case 2 - .js extension callback pattern",
      moduleType: "CJS",
      pattern: "callback",
      fileExtension: ".js", 
      loadMode: process.env.LOAD_ESM !== 'false' ? 'ESM' : 'CJS',
      delay: delay,
      testCase: "js-cjs-callback",
      timestamp: new Date().toISOString()
    }, null, 2), {
      "Content-Type": "application/json"
    });
  }, delay);
};