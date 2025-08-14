// ESM helloWorld test case for Node.js 22
export default async (context) => {
  return {
    status: 200,
    body: "hello, world from ESM! 🎉\n",
    headers: {
      "X-Module-Type": "ESM",
      "X-Node-Version": process.version
    }
  };
}; 