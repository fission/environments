// Both should work with env var set.

module.exports = async function (context) {
    console.log("headers=", JSON.stringify(context.request.headers));
    console.log("body=", JSON.stringify(context.request.body));

    return {
        status: 200,
        body: "Hello, world !\n"
    };
}
// export default async function (context) {
//     console.log("ESM headers=", JSON.stringify(context.request.headers));
//     console.log("ESM body=", JSON.stringify(context.request.body));
//     console.log("ESM method=", context.request.method);

//     return {
//         status: 200,
//         body: "Hello, world !\n",
//         headers: {
//             "X-Module-Type": "ESM",
//             "X-Node-Version": process.version
//         }
//     };
// }