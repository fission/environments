// ESM helloUser test case for Node.js 22
import { parse } from "url";

export default async (context) => {
  console.log("ESM Function - URL:", context.request.url);

  const urlParts = parse(context.request.url, true);
  const query = urlParts.query;

  console.log("ESM Function - query user:", query.user);

  return {
    status: 200,
    body: `hello ${query.user || 'anonymous'} from ESM Node.js 22! 🚀\n`,
    headers: {
      "X-Module-Type": "ESM",
      "Content-Type": "text/plain",
      "X-Node-Version": process.version
    }
  };
}; 