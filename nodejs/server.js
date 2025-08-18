const fs = require("fs");
const path = require("path");
const process = require("process");
const express = require("express");
const pino = require("pino");
const { createServer } = require("http");
const { WebSocketServer } = require("ws");
const minimist = require("minimist");

const DEFAULT_TIMEOUT = 60000;


// Initialize modern structured logger
const logger = pino({
  level: process.env.LOG_LEVEL || 'info'
});

// Module loading mode configuration
const LOAD_ESM = process.env.LOAD_ESM !== 'false'; // Default to true (ESM mode)
logger.info(`Module loading mode: ${LOAD_ESM ? 'ESM' : 'CJS'}`);



const app = express();
const argv = minimist(process.argv.slice(1)); // Command line opts

if (!argv.port) {
  argv.port = 8888;
}

// Interval at which we poll for connections to be active
let timeout;
if (process.env.TIMEOUT) {
  timeout = process.env.TIMEOUT;
} else {
  timeout = DEFAULT_TIMEOUT;
}

// To catch unhandled exceptions thrown by user code async callbacks,
// these exceptions cannot be catched by try-catch in user function invocation code below
process.on("uncaughtException", (err) => {
  console.error(`Caught exception: ${err}`);
});

// User function.  Starts out undefined.
let userFunction;

const loadFunction = async (modulepath, funcname) => {
  // Read and load the code. It's placed there securely by the fission runtime.
  try {
    let startTime = process.hrtime();
    // support v1 codepath and v2 entrypoint like 'foo', '', 'index.hello'
    let userModule;
    
    let importPath = modulepath;
    
    // Handle missing file extensions based on LOAD_ESM mode
    if (!path.extname(modulepath)) {
      const basePath = modulepath;
      let possibleExtensions;
      
      if (LOAD_ESM) {
        // ESM mode: prioritize .js (ESM), .mjs, then .cjs
        possibleExtensions = ['.js', '.mjs', '.cjs'];
      } else {
        // CJS mode: prioritize .js (CJS), .cjs, then .mjs  
        possibleExtensions = ['.js', '.cjs', '.mjs'];
      }
      
      let foundPath = null;
      for (const ext of possibleExtensions) {
        const testPath = basePath + ext;
        if (fs.existsSync(testPath)) {
          foundPath = testPath;
          break;
        }
      }
      
      importPath = foundPath || (basePath + '.js');
    }
    
    // Determine how to interpret .js files based on LOAD_ESM mode
    const isJsFile = importPath.endsWith('.js');
    const isCjsFile = importPath.endsWith('.cjs'); 
    const isMjsFile = importPath.endsWith('.mjs');
    
    // In ESM mode: .js = ESM, .mjs = ESM, .cjs = CJS
    // In CJS mode: .js = CJS, .cjs = CJS, .mjs = ESM
    const treatAsCjs = isCjsFile || (!LOAD_ESM && isJsFile);
    
    try {
      // Always use dynamic import() - Node.js handles both ESM and CJS correctly
      userModule = await import(importPath);
    } catch (importError) {
      try {
        // Try with file:// protocol for absolute paths
        let attempt2Path;
        if (path.isAbsolute(importPath)) {
          attempt2Path = `file://${importPath}`;
        } else {
          attempt2Path = `file://${path.resolve(importPath)}`;
        }
        userModule = await import(attempt2Path);
      } catch (fallbackError) {
        // Final fallback: try original path with file:// protocol
        const originalPath = path.isAbsolute(modulepath) 
          ? `file://${modulepath}` 
          : `file://${path.resolve(modulepath)}`;
        userModule = await import(originalPath);
      }
    }
    
    // Handle function extraction based on module type
    let userFunction;
    if (funcname) {
      // Named function requested
      userFunction = userModule[funcname] || userModule.default?.[funcname];
    } else {
      // Default export requested - handle based on module type
      if (treatAsCjs) {
        // CJS: module.exports becomes default export when imported
        userFunction = userModule.default || userModule;
      } else {
        // ESM: prefer default export
        userFunction = userModule.default || userModule;
      }
    }
      
    let elapsed = process.hrtime(startTime);
    console.log(
      `user code loaded in ${elapsed[0]}sec ${elapsed[1] / 1000000}ms (${LOAD_ESM ? 'ESM' : 'CJS'} mode)`
    );
    return userFunction;
  } catch (e) {
    console.error(`user code load error: ${e}`);
    return e;
  }
};

const withEnsureGeneric = (func) => {
  return (req, res) => {
    // Make sure we're a generic container.  (No reuse of containers.
    // Once specialized, the container remains specialized.)
    if (userFunction) {
      res.status(400).send("Not a generic container");
      return;
    }

    func(req, res);
  };
};

const isFunction = (func) => {
  return func && func.constructor && func.call && func.apply;
};

const specializeV2 = async (req, res) => {
  let filename, funcname;
  
  if (req.body.functionName && req.body.functionName.includes('.')) {
    // Standard format: 'filename.funcname' => ['filename', 'funcname']
    const entrypoint = req.body.functionName.split(".");
    filename = entrypoint[0];
    funcname = entrypoint[1];
  } else if (req.body.functionName && req.body.functionName.trim() !== '') {
    // Single word format: interpret as filename with default export
    filename = req.body.functionName;
    funcname = undefined; // Default export
  } else {
    // Empty functionName means default export - find the main file
    funcname = undefined; // Default export
  }
  
  // If we don't have a filename yet, find it from the directory or file
  if (!filename) {
    try {
      const stats = fs.statSync(req.body.filepath);
      
      if (stats.isFile()) {
        // The filepath points directly to a file
        filename = path.basename(req.body.filepath, path.extname(req.body.filepath));
      } else if (stats.isDirectory()) {
        // The filepath is a directory, look for files inside
        const allFiles = fs.readdirSync(req.body.filepath);
        
        const files = allFiles.filter(f => 
          f.endsWith('.js') || f.endsWith('.mjs') || f.endsWith('.cjs')
        );
        
        if (files.length === 1) {
          filename = path.parse(files[0]).name; // Remove file extension
        } else if (files.length > 1) {
          // Try to find the main file from package.json
          let mainFile = null;
          try {
            const packageJsonPath = path.join(req.body.filepath, 'package.json');
            if (fs.existsSync(packageJsonPath)) {
              const packageJson = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
              if (packageJson.main && files.includes(packageJson.main)) {
                mainFile = packageJson.main;
              }
            }
          } catch (err) {
            // Ignore package.json read errors
          }
          
          // Fallback to common patterns - prioritize index files
          if (!mainFile) {
            mainFile = files.find(f => f === 'index.js') || 
                      files.find(f => f === 'index.mjs') ||
                      files.find(f => f === 'index.cjs') ||
                      files.find(f => f === 'main.js') ||
                      files[0];
          }
          
          filename = path.parse(mainFile).name;
        } else {
          filename = 'index';
        }
      }
    } catch (err) {
      filename = 'index';
    }
  }
  
  // Construct the module path
  let modulepath;
  try {
    const stats = fs.statSync(req.body.filepath);
    if (stats.isFile()) {
      // If filepath is already a file, use it directly
      modulepath = req.body.filepath;
    } else {
      // If filepath is a directory, join with filename
      modulepath = path.join(req.body.filepath, filename);
    }
  } catch (err) {
    // Fallback: assume it's a directory
    modulepath = path.join(req.body.filepath, filename);
  }
  
  // Don't automatically add .js extension here - let loadFunction handle extension detection
  
  const result = await loadFunction(modulepath, funcname);

  if (isFunction(result)) {
    userFunction = result;
    res.status(202).send();
  } else {
    res.status(500).send(JSON.stringify(result));
  }
};

const specialize = async (req, res) => {
  // Specialize this server to a given user function.  The user function
  // is read from argv.codepath; it's expected to be placed there by the
  // fission runtime.
  let modulepath = argv.codepath || "/userfunc/user";

  // Node resolves module paths according to a file's location. We load
  // the file from argv.codepath, but tell users to put dependencies in
  // the server's package.json; this means the function's dependencies
  // are in /usr/src/app/node_modules.  We could be smarter and have the
  // function deps in the right place in argv.codepath; but for now we
  // just symlink the function's node_modules to the server's
  // node_modules.
  // Check for symlink, because the link exists if the container restarts
  const targetNodeModules = fs.existsSync("/usr/src/app/node_modules") 
    ? "/usr/src/app/node_modules" 
    : path.resolve(__dirname, "node_modules");
  
  if (!fs.existsSync(`${path.dirname(modulepath)}/node_modules`)) {
    try {
      fs.symlinkSync(
        targetNodeModules,
        `${path.dirname(modulepath)}/node_modules`
      );
    } catch (err) {
      if (err.code !== 'EEXIST') {
        logger.warn(`Could not create symlink: ${err.message}`);
      }
    }
  }
  
  // Enhanced: Handle extension detection for CJS support
  // If modulepath doesn't have an extension, try to find the right file
  if (!path.extname(modulepath)) {
    const basePath = modulepath;
    const possibleExtensions = ['.js', '.mjs', '.cjs'];
    let foundPath = null;
    
    for (const ext of possibleExtensions) {
      const testPath = basePath + ext;
      if (fs.existsSync(testPath)) {
        foundPath = testPath;
        break;
      }
    }
    
    if (foundPath) {
      modulepath = foundPath;
    }
  }
  
  const result = await loadFunction(modulepath);

  if (isFunction(result)) {
    userFunction = result;
    res.status(202).send();
  } else {
    res.status(500).send(JSON.stringify(result));
  }
};

// Request logger using pino
app.use((req, res, next) => {
  const start = Date.now();
  res.on('finish', () => {
    const duration = Date.now() - start;
    logger.info({
      method: req.method,
      url: req.url,
      status: res.statusCode,
      duration: `${duration}ms`,
      userAgent: req.get('User-Agent')
    }, 'HTTP Request');
  });
  next();
});

let bodyParserLimit = process.env.BODY_PARSER_LIMIT || "1mb";

app.use(express.urlencoded({ extended: false, limit: bodyParserLimit }));
app.use(express.json({ limit: bodyParserLimit }));
app.use(express.raw({ limit: bodyParserLimit }));
app.use(express.text({ type: "text/*", limit: bodyParserLimit }));

app.post("/specialize", withEnsureGeneric(specialize));
app.post("/v2/specialize", withEnsureGeneric(specializeV2));

// Generic route -- all http requests go to the user function.
app.use((req, res) => {
  if (!userFunction) {
    res.status(500).send("Generic container: no requests supported");
    return;
  }

  const context = {
    request: req,
    response: res,
    // TODO: context should also have: URL template params, query string
  };

  const callback = (status, body, headers) => {
    if (!status) return;
    if (headers) {
      for (let name of Object.keys(headers)) {
        res.set(name, headers[name]);
      }
    }
    res.status(status).send(body);
  };

  //
  // Customizing the request context
  //
  // If you want to modify the context to add anything to it,
  // you can do that here by adding properties to the context.
  //

  if (userFunction.length <= 1) {
    // One or zero argument (context)
    let result;
    // Make sure their function returns a promise
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
        logger.error(`Function error: ${err}`);
        callback(500, "Internal server error");
      });
  } else {
    // 2 arguments (context, callback)
    try {
      userFunction(context, callback);
    } catch (err) {
      logger.error(`Function error: ${err}`);
      callback(500, "Internal server error");
    }
  }
});

let server = createServer(app);

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

const heartbeat = function() {
  this.isAlive = true;
};
// warm indicates whether this pod has ever been active
let warm = false;

let interval;
interval = setInterval(() => {
  if (warm) {
    if (wss.clients.size > 0) {
      wss.clients.forEach((ws) => {
        // We check if all connections are alive
        if (ws.isAlive === false) return ws.terminate();

        ws.isAlive = false;
        // If client replies, we execute the hearbeat function(pong) and set the connection as active
        ws.ping(noop);
      });
    } else {
      // After we have pinged all clients and verified number of active connections is 0, we generate event for inactivity on the websocket
      fetch(wsInactiveEvent.url)
        .catch(err => {
          logger.error({ err }, "WebSocket inactive event failed");
          console.log(err);
        });
      return;
    }
  }
}, timeout);

wss.on("connection", (ws) => {
  if (warm == false) {
    warm = true;
    // On successful request, there's no body returned
    fetch(wsStartEvent.url)
      .then(res => {
        if (!res.ok) {
          logger.error({ status: res.status }, "WebSocket start event failed");
          console.log("Unexpected response");
          ws.send("Error");
        }
      })
      .catch(err => {
        logger.error({ err }, "WebSocket start event failed");
        console.log(err);
        ws.send("Error");
      });
  }

  ws.isAlive = true;
  ws.on("pong", heartbeat);

  wss.on("close", () => {
    clearInterval(interval);
  });

  try {
    userFunction(ws, wss.clients);
  } catch (err) {
    logger.error({ err }, "Function error");
    ws.close();
  }
});

server.listen(argv.port, () => {
  console.log(`Fission Node.js 22 runtime listening on port ${argv.port}`);
});
