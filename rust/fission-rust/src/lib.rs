//! Runtime SDK for writing [Fission](https://fission.io) functions in Rust.
//!
//! A Fission Rust function is a regular binary that serves HTTP on the
//! loopback port given by the `FISSION_RUNTIME_PORT` environment variable
//! (set by the environment's supervisor, which reverse-proxies requests
//! to it). This crate provides the minimal glue:
//!
//! ```no_run
//! use fission_rust::IntoResponse;
//!
//! async fn handler() -> impl IntoResponse {
//!     "Hello, World!\n"
//! }
//!
//! #[tokio::main]
//! async fn main() {
//!     fission_rust::serve(handler).await;
//! }
//! ```
//!
//! For multiple routes, middleware, or shared state, build your own
//! [`axum::Router`] and pass it to [`serve_router`]. The full axum API is
//! re-exported as [`axum`].

pub use axum;
pub use axum::response::IntoResponse;

use axum::Router;
use std::net::SocketAddr;
use tower_http::catch_panic::CatchPanicLayer;

const DEFAULT_PORT: u16 = 8889;

/// Resolve the port the function must listen on.
///
/// Reads `FISSION_RUNTIME_PORT` (set by the supervisor), then `PORT`,
/// falling back to 8889.
#[doc(hidden)]
pub fn port() -> u16 {
    std::env::var("FISSION_RUNTIME_PORT")
        .or_else(|_| std::env::var("PORT"))
        .ok()
        .and_then(|s| s.parse().ok())
        .unwrap_or(DEFAULT_PORT)
}

/// Serve a single handler for every path and method.
///
/// This matches Fission's model where the router maps a function to an
/// HTTP trigger and the function receives all requests routed to it.
pub async fn serve<H, T>(handler: H)
where
    H: axum::handler::Handler<T, ()>,
    T: 'static,
{
    serve_router(Router::new().fallback(handler)).await
}

/// Resolve the address to bind. Defaults to loopback (the supervisor
/// proxies over localhost); set `FISSION_RUNTIME_HOST=0.0.0.0` to expose
/// the function directly, e.g. when running the binary standalone.
fn host() -> std::net::IpAddr {
    std::env::var("FISSION_RUNTIME_HOST")
        .ok()
        .and_then(|s| s.parse().ok())
        .unwrap_or(std::net::IpAddr::from([127, 0, 0, 1]))
}

/// Serve a user-built [`Router`] (multiple routes, state, middleware).
pub async fn serve_router(router: Router) {
    // A panicking handler returns 500 and the process keeps serving;
    // the pod is not torn down by one bad request.
    let router = router.layer(CatchPanicLayer::new());
    let addr = SocketAddr::new(host(), port());
    let listener = tokio::net::TcpListener::bind(addr)
        .await
        .unwrap_or_else(|e| panic!("fission-rust: failed to bind {addr}: {e}"));
    eprintln!("fission-rust: listening on {addr}");
    axum::serve(listener, router)
        .await
        .expect("fission-rust: server error");
}
