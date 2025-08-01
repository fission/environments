// WebSocket broadcast example for Node.js 22 ESM
// Demonstrates broadcasting messages to all connected clients

export default async function(ws, clients) {
    // Handle incoming messages and broadcast to all clients
    ws.on('message', (data) => {
        const message = data.toString();
        
        // Create broadcast message with metadata
        const broadcastData = {
            type: 'broadcast',
            message,
            sender: ws.id || 'anonymous',
            timestamp: new Date().toISOString(),
            nodeVersion: process.version,
            totalClients: clients.size
        };
        
        const broadcastMessage = JSON.stringify(broadcastData);
        
        // Broadcast to all connected clients
        clients.forEach((client) => {
            if (client.readyState === client.OPEN) {
                client.send(broadcastMessage);
            }
        });
        
        console.log(`Broadcasting message to ${clients.size} clients: ${message}`);
    });
    
    // Send welcome message
    ws.send(JSON.stringify({
        type: 'welcome',
        message: 'Connected to Node.js 22 ESM WebSocket broadcast server! 📡',
        nodeVersion: process.version,
        clientCount: clients.size,
        timestamp: new Date().toISOString()
    }));
    
    // Notify other clients about new connection
    const joinMessage = JSON.stringify({
        type: 'user-joined',
        message: 'A new user joined the broadcast',
        timestamp: new Date().toISOString(),
        totalClients: clients.size
    });
    
    clients.forEach((client) => {
        if (client !== ws && client.readyState === client.OPEN) {
            client.send(joinMessage);
        }
    });
};
