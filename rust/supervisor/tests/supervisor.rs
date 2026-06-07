use std::fs;

use axum::body::Body;
use axum::http::{Request, StatusCode};
use http_body_util::BodyExt;
use supervisor::{DEFAULT_BINARY_NAME, Supervisor, app, resolve_binary};
use tower::ServiceExt;

// --- resolve_binary -----------------------------------------------------

#[test]
fn resolves_a_plain_file() {
    let dir = tempfile::tempdir().unwrap();
    let bin = dir.path().join("myfn");
    fs::write(&bin, b"#!/bin/sh\n").unwrap();
    assert_eq!(resolve_binary(&bin, "").unwrap(), bin);
    // function_name is irrelevant when filepath is already a file
    assert_eq!(resolve_binary(&bin, "other").unwrap(), bin);
}

#[test]
fn resolves_entrypoint_in_directory() {
    let dir = tempfile::tempdir().unwrap();
    fs::write(dir.path().join("alpha"), b"").unwrap();
    fs::write(dir.path().join("beta"), b"").unwrap();
    assert_eq!(
        resolve_binary(dir.path(), "beta").unwrap(),
        dir.path().join("beta")
    );
    assert!(resolve_binary(dir.path(), "missing").is_err());
}

#[test]
fn falls_back_to_default_binary_name() {
    let dir = tempfile::tempdir().unwrap();
    fs::write(dir.path().join(DEFAULT_BINARY_NAME), b"").unwrap();
    fs::write(dir.path().join("other"), b"").unwrap();
    assert_eq!(
        resolve_binary(dir.path(), "").unwrap(),
        dir.path().join(DEFAULT_BINARY_NAME)
    );
}

#[test]
fn accepts_a_sole_file_without_entrypoint() {
    let dir = tempfile::tempdir().unwrap();
    fs::write(dir.path().join("onlyone"), b"").unwrap();
    assert_eq!(
        resolve_binary(dir.path(), "").unwrap(),
        dir.path().join("onlyone")
    );
}

#[test]
fn rejects_ambiguous_and_empty_packages() {
    let dir = tempfile::tempdir().unwrap();
    assert!(resolve_binary(dir.path(), "").is_err(), "empty dir");
    fs::write(dir.path().join("a"), b"").unwrap();
    fs::write(dir.path().join("b"), b"").unwrap();
    assert!(resolve_binary(dir.path(), "").is_err(), "ambiguous dir");
    assert!(resolve_binary(&dir.path().join("nope"), "").is_err(), "missing path");
}

// --- HTTP contract ------------------------------------------------------

fn test_app() -> axum::Router {
    app(Supervisor::new(18889, "/nonexistent/userfunc/user"))
}

#[tokio::test]
async fn healthz_is_ok_before_specialize() {
    let res = test_app()
        .oneshot(Request::get("/healthz").body(Body::empty()).unwrap())
        .await
        .unwrap();
    assert_eq!(res.status(), StatusCode::OK);
}

#[tokio::test]
async fn requests_before_specialize_return_500() {
    let res = test_app()
        .oneshot(Request::post("/").body(Body::from("hi")).unwrap())
        .await
        .unwrap();
    assert_eq!(res.status(), StatusCode::INTERNAL_SERVER_ERROR);
    let body = res.into_body().collect().await.unwrap().to_bytes();
    assert!(String::from_utf8_lossy(&body).contains("not specialized"));
}

#[tokio::test]
async fn v2_specialize_with_bad_path_returns_500() {
    let req = Request::post("/v2/specialize")
        .header("content-type", "application/json")
        .body(Body::from(
            r#"{"filepath": "/no/such/path", "functionName": ""}"#,
        ))
        .unwrap();
    let res = test_app().oneshot(req).await.unwrap();
    assert_eq!(res.status(), StatusCode::INTERNAL_SERVER_ERROR);
}

#[tokio::test]
async fn v1_specialize_with_missing_code_path_returns_500() {
    let res = test_app()
        .oneshot(Request::post("/specialize").body(Body::empty()).unwrap())
        .await
        .unwrap();
    assert_eq!(res.status(), StatusCode::INTERNAL_SERVER_ERROR);
}
