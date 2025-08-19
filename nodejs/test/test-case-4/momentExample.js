// CJS momentExample test case for Node.js 22
const moment = require("moment");

module.exports = async (context) => {
  return {
    status: 200,
    body: JSON.stringify({
      message: "Hello from CJS Node.js 22! 🕐",
      timestamp: moment().format(),
      moduleType: "CJS",
      nodeVersion: process.version
    }),
    headers: {
      "Content-Type": "application/json",
      "X-Module-Type": "CJS"
    }
  };
}; 