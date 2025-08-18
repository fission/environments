module.exports = async (context) => {
  // Get the request body as string
  const requestText = (context.request && typeof context.request.body !== "undefined")
  ? context.request.body
  : (typeof context.request === "string" ? context.request : "");

  const splitStringArray = requestText.toString().split(" ");

  return {
    status: 200,
    body: JSON.stringify({
      wordCount: splitStringArray.length,
      words: splitStringArray,
      moduleType: "CJS",
      nodeVersion: process.version
    }),
    headers: {
      "Content-Type": "application/json"
    }
  };
};
