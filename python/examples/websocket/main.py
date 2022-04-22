from flask import request
from flask import current_app
from flask import g

print("loaded code")

def main(ws):
    print("main")
    while not ws.closed:
        message = ws.receive()
        ws.send(message)
