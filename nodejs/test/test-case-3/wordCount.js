module.exports = async (context) => {
  // Get the request body as string
  let requestText = "";
  
  if (context.request && context.request.body) {
    // Handle different body types
    if (typeof context.request.body === 'string') {
      requestText = context.request.body;
    } else if (typeof context.request.body === 'object') {
      // When body is parsed as form data, get the first key
      const keys = Object.keys(context.request.body);
      if (keys.length > 0) {
        // Reconstruct the original string from form data keys
        requestText = keys.join(' ');
      }
    }
  }

  const splitStringArray = requestText.toString().split(" ").filter(word => word.length > 0);

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
