use supervisor::{FUNCTION_PORT, Supervisor, V1_CODE_PATH, app};

const LISTEN_ADDR: &str = "0.0.0.0:8888";

#[tokio::main]
async fn main() {
    let state = Supervisor::new(FUNCTION_PORT, V1_CODE_PATH);
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
