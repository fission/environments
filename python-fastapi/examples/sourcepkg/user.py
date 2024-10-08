from yaml import load, dump
from fastapi import Request, Response

try:
    from yaml import CLoader as Loader, CDumper as Dumper
except ImportError:
    from yaml import Loader, Dumper

document = """
  a: 1
  b:
    c: 3
    d: 4
"""


def main(request: Request):
    return Response(dump(load(document, Loader=Loader), default_flow_style=None, Dumper=Dumper))
