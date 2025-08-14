// ESM momentExample test case for Node.js 22
import moment from "moment";

export default async (context) => {
  return {
    status: 200,
    body: JSON.stringify({
      message: "Hello from ESM Node.js 22! 🕐",
      timestamp: moment().format(),
      moduleType: "ESM",
      nodeVersion: process.version
    }),
    headers: {
      "Content-Type": "application/json",
      "X-Module-Type": "ESM"
    }
  };
}; 