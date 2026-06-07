// Env vars are process-global, so all port cases run in a single test
// to avoid races between parallel test threads.
#[test]
fn port_resolution() {
    unsafe {
        std::env::remove_var("FISSION_RUNTIME_PORT");
        std::env::remove_var("PORT");
    }
    assert_eq!(fission_rust::port(), 8889, "default port");

    unsafe { std::env::set_var("PORT", "9001") };
    assert_eq!(fission_rust::port(), 9001, "PORT fallback");

    unsafe { std::env::set_var("FISSION_RUNTIME_PORT", "9002") };
    assert_eq!(fission_rust::port(), 9002, "FISSION_RUNTIME_PORT wins over PORT");

    unsafe { std::env::set_var("FISSION_RUNTIME_PORT", "not-a-port") };
    unsafe { std::env::remove_var("PORT") };
    assert_eq!(fission_rust::port(), 8889, "garbage value falls back to default");
}
