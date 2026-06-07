use axum::{Json, Router, response::IntoResponse};
use serde_json::{Value, json};

async fn echo(body: Option<Json<Value>>) -> impl IntoResponse {
    Json(json!({ "echo": body.map(|Json(v)| v).unwrap_or(Value::Null) }))
}

#[tokio::main]
async fn main() {
    let port: u16 = std::env::var("FISSION_RUNTIME_PORT")
        .ok()
        .and_then(|p| p.parse().ok())
        .unwrap_or(8889);
    let listener = tokio::net::TcpListener::bind(("127.0.0.1", port))
        .await
        .expect("bind function port");
    axum::serve(listener, Router::new().fallback(echo))
        .await
        .expect("server error");
}
