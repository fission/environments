from fastapi import Request, Response

def entrypoint(request: Request):
    return Response('THIS_IS_ALTMAIN_ENTRYPOINT')
