from flask import request
from flask import current_app
from flask import g

print("loaded code")

def main(ws):
    print("main")
    count = 0
    while not ws.closed and count < 5:
        message = ws.receive()
        ws.send(message)
        count += 1
    ws.close()