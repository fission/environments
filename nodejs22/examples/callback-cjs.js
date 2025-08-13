// CommonJS callback pattern using .js extension
// Use with LOAD_ESM=false environment
module.exports = function(context, callback) {
  const name = context.request.query.name || 'World';
  
  setTimeout(() => {
    callback(200, JSON.stringify({
      message: `Hello ${name} from CommonJS callback with .js extension!`,
      nodeVersion: process.version,
      moduleType: "CJS",
      pattern: "callback",
      fileExtension: ".js",
      loadMode: process.env.LOAD_ESM !== 'false' ? 'ESM' : 'CJS',
      timestamp: new Date().toISOString()
    }, null, 2), {
      "Content-Type": "application/json"
    });
  }, 50);
};