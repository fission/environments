from fastapi import Request, Response

def main(request: Request):
    return Response('THIS_IS_MAIN_MAIN')

def func(request: Request):
    return Response('THIS_IS_MAIN_FUNC')
