#!/bin/bash
set -exo pipefail

# The deploy dir is not guaranteed to exist (see fission/environments#81).
mkdir -p "${DEPLOY_PKG}"

export CARGO_TARGET_DIR=/usr/src/fission/target
export CARGO_INCREMENTAL=0
export CARGO_TERM_COLOR=never

# Builds within one builder pod share the template workspace and target
# dir; serialize them.
exec 9>/usr/src/fission/.build.lock
flock 9

# Run `cargo build --release`, printing the produced binary paths on
# stdout (diagnostics still reach the build log via stderr).
cargo_build_binaries() {
    cargo build --release --message-format=json-render-diagnostics "$@" |
        jq -r 'select(.reason == "compiler-artifact")
               | select(.target.kind | index("bin"))
               | .executable // empty'
}

if [ -f "${SRC_PKG}/Cargo.toml" ]; then
    # --- Project mode: the source package is a Cargo project ---
    cd "${SRC_PKG}"
    bins=$(cargo_build_binaries)
else
    # --- Single-file mode: bare .rs file(s) wrapped in the template crate ---
    template_src=/usr/src/fission/template/src
    find "${template_src}" -name '*.rs' -delete
    if [ -f "${SRC_PKG}" ]; then
        cp "${SRC_PKG}" "${template_src}/handler.rs"
    elif [ -f "${SRC_PKG}/handler.rs" ]; then
        # handler.rs plus optional extra module files
        find "${SRC_PKG}" -maxdepth 1 -name '*.rs' ! -name main.rs \
            -exec cp {} "${template_src}/" \;
    else
        rs_files=("${SRC_PKG}"/*.rs)
        if [ ${#rs_files[@]} -ne 1 ] || [ ! -f "${rs_files[0]}" ]; then
            echo "Error: single-file packages need exactly one .rs file" \
                "or a handler.rs; use a Cargo project for anything bigger"
            exit 1
        fi
        cp "${rs_files[0]}" "${template_src}/handler.rs"
    fi

    # Generate the entrypoint, declaring every user file as a module so
    # handler.rs can reference siblings via `crate::<module>`. The module
    # list is collected before the redirect creates main.rs.
    mods=$(for f in "${template_src}"/*.rs; do
        echo "mod $(basename "${f}" .rs);"
    done)
    {
        echo "${mods}"
        cat <<'EOF'

#[tokio::main]
async fn main() {
    fission_rust::serve(handler::handler).await;
}
EOF
    } > "${template_src}/main.rs"

    cd /usr/src/fission/template
    # Dependencies are pre-fetched in the image; stay offline if possible.
    bins=$(cargo_build_binaries --offline) || {
        echo "Warning: offline build failed; retrying with network access." \
            "Single-file functions should only need the pre-baked crates" \
            "(fission-rust, axum, tokio, serde, serde_json)."
        bins=$(cargo_build_binaries)
    }
fi

count=$(echo "${bins}" | grep -c . || true)
if [ "${count}" -eq 0 ]; then
    echo "Error: the build produced no binaries; define a [[bin]] target"
    exit 1
elif [ "${count}" -eq 1 ]; then
    # Single binary: deploy under the default name the runtime expects.
    cp "${bins}" "${DEPLOY_PKG}/handler"
else
    # Multiple binaries: deploy all; the function's --entrypoint selects
    # one at specialize time.
    echo "Note: multiple binaries built; create functions with" \
        "--entrypoint <binary-name> to pick one:"
    for bin in ${bins}; do
        echo "  $(basename "${bin}")"
        cp "${bin}" "${DEPLOY_PKG}/$(basename "${bin}")"
    done
fi

chmod 0755 "${DEPLOY_PKG}"/*
echo "Build output:"
ls -l "${DEPLOY_PKG}"
