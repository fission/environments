// Placeholder handler; replaced by the user's .rs file at build time.
use fission_rust::IntoResponse;

pub async fn handler() -> impl IntoResponse {
    "hello from the fission rust template\n"
}
