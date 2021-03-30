module.exports = async function(ws, clients) {
   
    ws.on('message', message => {
        ws.send(message)
    });

    ws.on('close', function close() {
        return {
            status: 200,
            message: "I am done"
        }
    });
}
