// ESM test function
export default async function (context) {
    console.log("ESM headers=", JSON.stringify(context.request.headers));
    console.log("ESM body=", JSON.stringify(context.request.body));
    console.log("ESM method=", context.request.method);

    return {
        status: 200,
        body: "Hello from ESM Node.js 22! 🚀\n",
        headers: {
            "X-Module-Type": "ESM",
            "X-Node-Version": process.version
        }
    };
} 