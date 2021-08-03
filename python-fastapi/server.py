#!/usr/bin/env python
import asyncio
import importlib
import logging
import os
import sys

from fastapi import FastAPI, HTTPException, Request

try:
    LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")
    LOG_LEVEL = getattr(logging, LOG_LEVEL)
except:
    LOG_LEVEL = logging.INFO


def import_src(path):
    return importlib.machinery.SourceFileLoader("mod", path).load_module()


class FuncApp(FastAPI):
    def __init__(self, loglevel=logging.DEBUG):
        super(FuncApp, self).__init__()

        # init the class members
        self.userfunc = None
        self.logger = logging.getLogger()
        self.ch = logging.StreamHandler(sys.stdout)

        #
        # Logging setup.  TODO: Loglevel hard-coded for now. We could allow
        # functions/routes to override this somehow; or we could create
        # separate dev vs. prod environments.
        #
        self.logger.setLevel(loglevel)
        self.ch.setLevel(loglevel)
        self.ch.setFormatter(
            logging.Formatter("%(asctime)s - %(levelname)s - %(message)s")
        )
        self.logger.addHandler(self.ch)

        #
        # Register the routers
        #
        @self.api_route("/specialize", methods=["POST"])
        async def load():
            self.logger.info("/specialize called")
            # load user function from codepath
            self.userfunc = import_src("/userfunc/user").main
            return ""

        @self.api_route("/v2/specialize", methods=["POST"])
        async def loadv2(request: Request):
            body = await request.json()
            filepath = body["filepath"]
            handler = body["functionName"]
            self.logger.info(
                '/v2/specialize called with  filepath = "{}"   handler = "{}"'.format(
                    filepath, handler
                )
            )

            # handler looks like `path.to.module.function`
            parts = handler.rsplit(".", 1)
            if len(handler) == 0:
                # default to main.main if entrypoint wasn't provided
                moduleName = "main"
                funcName = "main"
            elif len(parts) == 1:
                moduleName = "main"
                funcName = parts[0]
            else:
                moduleName = parts[0]
                funcName = parts[1]
            self.logger.debug(
                'moduleName = "{}"    funcName = "{}"'.format(moduleName, funcName)
            )

            # check whether the destination is a directory or a file
            if os.path.isdir(filepath):
                # add package directory path into module search path
                sys.path.append(filepath)

                self.logger.debug('__package__ = "{}"'.format(__package__))
                if __package__:
                    mod = importlib.import_module(moduleName, __package__)
                else:
                    mod = importlib.import_module(moduleName)

            else:
                # load source from destination python file
                mod = import_src(filepath)

            # load user function from module
            self.userfunc = getattr(mod, funcName)

            return ""

        @self.api_route("/healthz", methods=["GET"])
        async def healthz():
            return "", 200

        @self.api_route(
            "/", methods=["GET", "POST", "PUT", "HEAD", "OPTIONS", "DELETE"]
        )
        async def f(request: Request):
            if self.userfunc is None:
                print("Generic container: no requests supported")
                raise HTTPException(status_code=500)

            if asyncio.iscoroutinefunction(self.userfunc):
                return await self.userfunc(request)
            else:
                return self.userfunc(request)


app = FuncApp(LOG_LEVEL)


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8888, log_level=LOG_LEVEL)
