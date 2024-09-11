from fastapi import Request, Response

def bar(request: Request):
    return Response('THIS_IS_FOO_BAR')
