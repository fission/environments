//! Fission Rust environment supervisor.
//!
//! Implements the Fission environment contract on port 8888:
//!
//! - `GET  /healthz`        — readiness/liveness probe
//! - `POST /specialize`     — v1: load the function from /userfunc/user
//! - `POST /v2/specialize`  — v2: JSON `{filepath, functionName}`
//! - everything else        — reverse-proxied to the function process
//!
//! The function (a compiled server binary produced by the rust builder) is
//! spawned exactly once at specialize time and serves HTTP on
//! `127.0.0.1:8889`; all subsequent requests are streamed to it through a
//! pooled keep-alive client. If the function process exits, the supervisor
//! exits too so Fission replaces the pod (a container specializes once).

use std::path::{Path, PathBuf};
use std::sync::Arc;
use std::sync::atomic::{AtomicBool, Ordering};

use axum::{
    Router,
    body::Body,
    extract::{Request, State},
    http::{HeaderMap, StatusCode, Uri, header},
    response::{IntoResponse, Response},
    routing::{get, post},
};
use hyper_util::client::legacy::{Client, connect::HttpConnector};
use hyper_util::rt::TokioExecutor;
use serde::Deserialize;
use tokio::sync::Mutex;

/// Fixed path where the fetcher places the function for v1 specialize.
pub const V1_CODE_PATH: &str = "/userfunc/user";
/// Default binary name emitted by the builder's build.sh.
pub const DEFAULT_BINARY_NAME: &str = "handler";
/// Loopback port the function binary listens on (passed via env).
pub const FUNCTION_PORT: u16 = 8889;

#[derive(Debug, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct FunctionLoadRequest {
    #[serde(default)]
    pub filepath: String,
    #[serde(default)]
    pub function_name: String,
}

pub struct Supervisor {
    /// One-way latch: flips to true once the function is serving.
    ready: AtomicBool,
    /// Serializes specialize attempts; never touched on the proxy path.
    specialize_lock: Mutex<()>,
    client: Client<HttpConnector, Body>,
    function_port: u16,
    v1_code_path: PathBuf,
}

impl Supervisor {
    pub fn new(function_port: u16, v1_code_path: impl Into<PathBuf>) -> Arc<Self> {
        Arc::new(Self {
            ready: AtomicBool::new(false),
            specialize_lock: Mutex::new(()),
            client: Client::builder(TokioExecutor::new()).build_http(),
            function_port,
            v1_code_path: v1_code_path.into(),
        })
    }
}

/// Resolve the function binary inside a deploy package.
///
/// `path` may be the binary itself or a directory (an extracted deploy
/// package). For directories: `function_name` selects the binary by name
/// (the function's `--entrypoint`); otherwise the builder's default name
/// `handler` is used; otherwise a sole regular file is accepted.
pub fn resolve_binary(path: &Path, function_name: &str) -> Result<PathBuf, String> {
    let meta = std::fs::metadata(path)
        .map_err(|e| format!("cannot stat function path {}: {e}", path.display()))?;
    if meta.is_file() {
        return Ok(path.to_path_buf());
    }

    if !function_name.is_empty() {
        let candidate = path.join(function_name);
        return if candidate.is_file() {
            Ok(candidate)
        } else {
            Err(format!(
                "entrypoint {function_name:?} not found in deploy package {}",
                path.display()
            ))
        };
    }

    let default = path.join(DEFAULT_BINARY_NAME);
    if default.is_file() {
        return Ok(default);
    }

    let files: Vec<PathBuf> = std::fs::read_dir(path)
        .map_err(|e| format!("cannot read deploy package {}: {e}", path.display()))?
        .filter_map(|entry| entry.ok())
        .map(|entry| entry.path())
        .filter(|p| p.is_file())
        .collect();
    match files.as_slice() {
        [single] => Ok(single.clone()),
        [] => Err(format!("deploy package {} is empty", path.display())),
        _ => Err(format!(
            "deploy package {} has multiple files and no entrypoint; \
             set --entrypoint to the binary name",
            path.display()
        )),
    }
}

/// Spawn the function binary and wait until it accepts TCP connections.
async fn start_function(binary: &Path, port: u16) -> Result<(), String> {
    // The fetcher may not preserve the executable bit; best-effort fix.
    #[cfg(unix)]
    {
        use std::os::unix::fs::PermissionsExt;
        let _ = std::fs::set_permissions(binary, std::fs::Permissions::from_mode(0o755));
    }

    let workdir = binary.parent().unwrap_or(Path::new("/"));
    let mut child = tokio::process::Command::new(binary)
        .current_dir(workdir)
        .env("FISSION_RUNTIME_PORT", port.to_string())
        .env("PORT", port.to_string())
        // stdout/stderr inherited -> function logs land in container logs
        .spawn()
        .map_err(|e| format!("failed to start function {}: {e}", binary.display()))?;

    // Readiness: poll until the function's port accepts connections.
    let deadline = tokio::time::Instant::now() + tokio::time::Duration::from_secs(30);
    loop {
        if let Ok(Some(status)) = child.try_wait() {
            return Err(format!("function exited during startup: {status}"));
        }
        match tokio::net::TcpStream::connect(("127.0.0.1", port)).await {
            Ok(_) => break,
            Err(_) if tokio::time::Instant::now() < deadline => {
                tokio::time::sleep(tokio::time::Duration::from_millis(50)).await;
            }
            Err(e) => {
                let _ = child.start_kill();
                return Err(format!("function did not become ready in 30s: {e}"));
            }
        }
    }

    // Liveness: if the function process ever exits, exit the supervisor so
    // Fission replaces the pod — a container specializes exactly once.
    tokio::spawn(async move {
        let status = child.wait().await;
        eprintln!("supervisor: function process exited: {status:?}");
        std::process::exit(1);
    });
    Ok(())
}

async fn specialize(supervisor: &Supervisor, path: &Path, function_name: &str) -> Response {
    let _guard = supervisor.specialize_lock.lock().await;
    if supervisor.ready.load(Ordering::Acquire) {
        return error_response(StatusCode::BAD_REQUEST, "Not a generic container");
    }
    // A bad deploy package is the client's mistake (matches binary env's 400).
    let binary = match resolve_binary(path, function_name) {
        Ok(binary) => binary,
        Err(err) => return error_response(StatusCode::BAD_REQUEST, &err),
    };
    eprintln!("supervisor: specializing with {}", binary.display());
    match start_function(&binary, supervisor.function_port).await {
        Ok(()) => {
            supervisor.ready.store(true, Ordering::Release);
            StatusCode::OK.into_response()
        }
        Err(err) => error_response(StatusCode::INTERNAL_SERVER_ERROR, &err),
    }
}

fn error_response(status: StatusCode, message: &str) -> Response {
    eprintln!("supervisor: {message}");
    (status, message.to_string()).into_response()
}

async fn specialize_v1(State(supervisor): State<Arc<Supervisor>>) -> Response {
    let path = supervisor.v1_code_path.clone();
    specialize(&supervisor, &path, "").await
}

async fn specialize_v2(
    State(supervisor): State<Arc<Supervisor>>,
    body: axum::Json<FunctionLoadRequest>,
) -> Response {
    // An empty filepath falls back to the v1 fetch location, like the
    // binary and go environments do.
    let path = if body.filepath.is_empty() {
        supervisor.v1_code_path.clone()
    } else {
        PathBuf::from(&body.filepath)
    };
    specialize(&supervisor, &path, &body.function_name).await
}

async fn healthz() -> StatusCode {
    StatusCode::OK
}

/// Hop-by-hop headers must not be forwarded by a proxy (RFC 9110 §7.6.1).
fn strip_hop_by_hop_headers(headers: &mut HeaderMap) {
    for name in [
        header::CONNECTION,
        header::PROXY_AUTHENTICATE,
        header::PROXY_AUTHORIZATION,
        header::TE,
        header::TRAILER,
        header::TRANSFER_ENCODING,
        header::UPGRADE,
    ] {
        headers.remove(name);
    }
    headers.remove("keep-alive");
    headers.remove("proxy-connection");
}

/// Reverse-proxy a request to the function, streaming both bodies.
async fn proxy(State(supervisor): State<Arc<Supervisor>>, mut req: Request) -> Response {
    if !supervisor.ready.load(Ordering::Acquire) {
        return error_response(StatusCode::INTERNAL_SERVER_ERROR, "Container not specialized");
    }

    let path_and_query = req
        .uri()
        .path_and_query()
        .map(|pq| pq.as_str())
        .unwrap_or("/");
    let uri = format!("http://127.0.0.1:{}{}", supervisor.function_port, path_and_query);
    *req.uri_mut() = match uri.parse::<Uri>() {
        Ok(uri) => uri,
        Err(e) => {
            return error_response(StatusCode::BAD_REQUEST, &format!("bad request uri: {e}"));
        }
    };
    strip_hop_by_hop_headers(req.headers_mut());

    match supervisor.client.request(req).await {
        Ok(mut response) => {
            strip_hop_by_hop_headers(response.headers_mut());
            response.map(Body::new).into_response()
        }
        Err(e) => error_response(StatusCode::BAD_GATEWAY, &format!("function unreachable: {e}")),
    }
}

pub fn app(supervisor: Arc<Supervisor>) -> Router {
    Router::new()
        .route("/healthz", get(healthz))
        .route("/specialize", post(specialize_v1))
        .route("/v2/specialize", post(specialize_v2))
        .fallback(proxy)
        .with_state(supervisor)
}
