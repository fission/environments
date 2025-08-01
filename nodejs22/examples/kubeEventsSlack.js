// Kubernetes Events to Slack Integration for Node.js 22 ESM
import https from 'https';

export default async (context) => {
    // Replace this with your actual Slack webhook URL
    const slackWebhookPath = '/services/YOUR/SLACK/WEBHOOK';
    const slackHost = 'hooks.slack.com';
    
    const { request } = context;
    const event = request.body;
    
    if (!event || !event.object) {
        return {
            status: 400,
            body: JSON.stringify({
                error: 'Invalid Kubernetes event',
                received: event
            }),
            headers: {
                'Content-Type': 'application/json'
            }
        };
    }
    
    // Extract event information
    const eventObj = event.object;
    const eventType = event.type || 'Unknown';
    const reason = eventObj.reason || 'No reason';
    const message = eventObj.message || 'No message';
    const namespace = eventObj.namespace || 'default';
    const name = eventObj.involvedObject?.name || 'Unknown';
    const kind = eventObj.involvedObject?.kind || 'Unknown';
    
    // Format Slack message
    const slackMessage = {
        text: `🚨 Kubernetes Event: ${eventType}`,
        attachments: [{
            color: eventType === 'Warning' ? 'danger' : 'good',
            fields: [
                { title: 'Reason', value: reason, short: true },
                { title: 'Object', value: `${kind}/${name}`, short: true },
                { title: 'Namespace', value: namespace, short: true },
                { title: 'Message', value: message, short: false }
            ],
            timestamp: Math.floor(Date.now() / 1000)
        }]
    };
    
    const postData = JSON.stringify(slackMessage);
    
    const options = {
        hostname: slackHost,
        port: 443,
        path: slackWebhookPath,
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Content-Length': Buffer.byteLength(postData)
        }
    };
    
    return new Promise((resolve) => {
        const req = https.request(options, (res) => {
            let responseBody = '';
            
            res.on('data', (chunk) => {
                responseBody += chunk;
            });
            
            res.on('end', () => {
                resolve({
                    status: res.statusCode,
                    body: JSON.stringify({
                        success: res.statusCode === 200,
                        message: 'Event sent to Slack',
                        slackResponse: responseBody,
                        event: {
                            type: eventType,
                            reason,
                            object: `${kind}/${name}`,
                            namespace
                        }
                    }),
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });
            });
        });
        
        req.on('error', (error) => {
            resolve({
                status: 500,
                body: JSON.stringify({
                    success: false,
                    error: 'Failed to send to Slack',
                    details: error.message
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            });
        });
        
        req.write(postData);
        req.end();
    });
};