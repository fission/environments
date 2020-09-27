use std::env;

fn main() {
    println!("Hello, world!");
    for (key, value) in env::vars() {
        println!("{}: {}", key, value);
    }
}
