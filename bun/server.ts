import fs from "fs";
import path from "path";
import process from "process";
import type { Request, Response } from "express";
import express from "express";
import request from "request";
import morgan from "morgan";
import { WebSocketServer, WebSocket } from "ws";
import minimist from "minimist";
import http from "http";

const app = express();
const argv = minimist(process.argv.slice(1)); // Command line opts

if (!argv["port"]) {
  argv["port"] = 8888;
}

// Interval at which we poll for connections to be active
let timeout: number;
if (process.env["TIMEOUT"]) {
  timeout = parseInt(process.env["TIMEOUT"], 10);
} else {
  timeout = 60000;
}

// To catch unhandled exceptions thrown by user code async callbacks,
// these exceptions cannot be caught by try-catch in user function invocation code below
process.on("uncaughtException", (err) => {
  console.error(`Caught exception: ${err}`);
});

// User function. Starts out undefined.
let userFunction: Function | undefined;

const loadFunction = async (modulepath: string, funcname?: string) => {
  try {
    let startTime = process.hrtime();
    const pkg = await import(modulepath);
    let userFunction = funcname ? pkg[funcname] : pkg.default;

    let elapsed = process.hrtime(startTime);
    console.log(
      `user code loaded in ${elapsed[0]}sec ${elapsed[1] / 1000000}ms`
    );

    return userFunction;
  } catch (e: any) {
    console.error(`user code load error: ${e}`);
    return e;
  }
};

const withEnsureGeneric = (
  func: (req: Request, res: Response) => Promise<void>
) => {
  return (req: Request, res: Response) => {
    if (userFunction) {
      res.status(400).send("Not a generic container");
      return;
    }
    func(req, res);
  };
};

const isFunction = (func: any): func is Function => {
  return func && func.constructor && func.call && func.apply;
};

const specializeV2 = async (req: Request, res: Response) => {
  const entrypoint = req.body.functionName
    ? req.body.functionName.split(".")
    : [];
  const modulepath = path.join(req.body.filepath, entrypoint[0] || "");
  const result = await loadFunction(modulepath, entrypoint[1]);

  if (isFunction(result)) {
    userFunction = result;
    res.status(202).send();
  } else {
    res.status(500).send(JSON.stringify(result));
  }
};

const specialize = async (req: Request, res: Response) => {
  const modulepath = argv["codepath"] || "/userfunc/user";

  if (!fs.existsSync(`${path.dirname(modulepath)}/node_modules`)) {
    fs.symlinkSync(
      "/usr/src/app/node_modules",
      `${path.dirname(modulepath)}/node_modules`
    );
  }
  const result = await loadFunction(modulepath);

  if (isFunction(result)) {
    userFunction = result;
    res.status(202).send();
  } else {
    res.status(500).send(JSON.stringify(result));
  }
};

// Request logger
app.use(morgan("combined"));

let bodyParserLimit = process.env["BODY_PARSER_LIMIT"] || "1mb";

app.use(express.urlencoded({ extended: false, limit: bodyParserLimit }));
app.use(express.json({ limit: bodyParserLimit }));
app.use(express.raw({ limit: bodyParserLimit }));
app.use(express.text({ type: "text/*", limit: bodyParserLimit }));

app.post("/specialize", withEnsureGeneric(specialize));
app.post("/v2/specialize", withEnsureGeneric(specializeV2));

// Generic route -- all http requests go to the user function.
app.all("*", (req: Request, res: Response) => {
  if (!userFunction) {
    res.status(500).send("Generic container: no requests supported");
    return;
  }

  const context = {
    request: req,
    response: res,
  };

  const callback = (
    status: number,
    body: any,
    headers?: { [key: string]: string }
  ) => {
    if (!status) return;
    if (headers) {
      for (let name of Object.keys(headers)) {
        res.set(name, headers[name]);
      }
    }
    res.status(status).send(body);
  };

  if (userFunction.length <= 1) {
    let result: Promise<any>;
    if (userFunction.length === 0) {
      result = Promise.resolve(userFunction());
    } else {
      result = Promise.resolve(userFunction(context));
    }
    result
      .then(({ status, body, headers }) => {
        callback(status, body, headers);
      })
      .catch((err) => {
        console.log(`Function error: ${err}`);
        callback(500, "Internal server error");
      });
  } else {
    try {
      userFunction(context, callback);
    } catch (err) {
      console.log(`Function error: ${err}`);
      callback(500, "Internal server error");
    }
  }
});

let server = http.createServer();

// Also mount the app here
server.on("request", app);

const wsStartEvent = {
  url: "http://127.0.0.1:8000/wsevent/start",
};

const wsInactiveEvent = {
  url: "http://127.0.0.1:8000/wsevent/end",
};

// Create web socket server on top of a regular http server
let wss = new WebSocketServer({
  server: server,
});

const noop = () => {};

const heartbeat = function (this: WebSocket) {
  (this as any).isAlive = true;
};

let warm = false;

let interval = setInterval(() => {
  if (warm) {
    if (wss.clients.size > 0) {
      wss.clients.forEach((ws) => {
        if ((ws as any).isAlive === false) return ws.terminate();

        (ws as any).isAlive = false;
        ws.ping(noop);
      });
    } else {
      request(wsInactiveEvent, (err, res) => {
        if (err || res.statusCode != 200) {
          if (err) {
            console.log(err);
          } else {
            console.log("Unexpected response");
          }
          return;
        }
      });
      return;
    }
  }
}, timeout);

wss.on("connection", (ws) => {
  if (warm == false) {
    warm = true;
    request(wsStartEvent, (err, res) => {
      if (err || res.statusCode != 200) {
        if (err) {
          console.log(err);
        } else {
          console.log("Unexpected response");
        }
        return;
      }
    });
  }

  (ws as any).isAlive = true;
  ws.on("pong", heartbeat);

  wss.on("close", () => {
    clearInterval(interval);
  });

  try {
    userFunction?.(ws, wss.clients);
  } catch (err) {
    console.log(`Function error: ${err}`);
    ws.close();
  }
});

server.listen(argv["port"], () => {});
