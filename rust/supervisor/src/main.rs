use supervisor::{FUNCTION_PORT, Supervisor, V1_CODE_PATH, app};

const LISTEN_ADDR: &str = "0.0.0.0:8888";

#[tokio::main]
async fn main() {
    // Defaults match the Fission contract; env vars allow non-standard
    // deployments without rebuilding the image.
    let function_port = std::env::var("FISSION_FUNCTION_PORT")
        .ok()
        .and_then(|p| p.parse().ok())
        .unwrap_or(FUNCTION_PORT);
    let v1_code_path =
        std::env::var("FISSION_V1_CODE_PATH").unwrap_or_else(|_| V1_CODE_PATH.to_string());

    let state = Supervisor::new(function_port, v1_code_path);
    let listener = tokio::net::TcpListener::bind(LISTEN_ADDR)
        .await
        .unwrap_or_else(|e| panic!("supervisor: failed to bind {LISTEN_ADDR}: {e}"));
    eprintln!("supervisor: listening on {LISTEN_ADDR}");

    let server = axum::serve(listener, app(state));
    tokio::select! {
        result = server => result.expect("supervisor: server error"),
        _ = shutdown_signal() => eprintln!("supervisor: shutting down"),
    }
}

async fn shutdown_signal() {
    let mut sigterm = tokio::signal::unix::signal(tokio::signal::unix::SignalKind::terminate())
        .expect("install SIGTERM handler");
    tokio::select! {
        _ = sigterm.recv() => {}
        _ = tokio::signal::ctrl_c() => {}
    }
}
