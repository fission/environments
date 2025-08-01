// ESM wordCount test case for Node.js 22
export default async (context) => {
  // Get the request body as string
  const requestText = context.request.body || context.request.rawBody || "";
  const splitStringArray = requestText.toString().split(" ");

  return {
    status: 200,
    body: JSON.stringify({
      wordCount: splitStringArray.length,
      words: splitStringArray,
      moduleType: "ESM",
      nodeVersion: process.version
    }),
    headers: {
      "Content-Type": "application/json",
      "X-Module-Type": "ESM"
    }
  };
}; 