// Entrypoint for single-file functions. The user's file is copied to
// src/handler.rs by the builder and must define:
//
//     pub async fn handler(...) -> impl IntoResponse
//
// (any axum handler signature works: extractors, Json, etc.)
mod handler;

#[tokio::main]
async fn main() {
    fission_rust::serve(handler::handler).await;
}
