#!/bin/bash
set -ex

# The deploy dir is not guaranteed to exist (see fission/environments#81).
mkdir -p "${DEPLOY_PKG}"

export CARGO_TARGET_DIR=/usr/src/fission/target
export CARGO_INCREMENTAL=0
export CARGO_TERM_COLOR=never

# Builds within one builder pod share the template workspace and target
# dir; serialize them.
exec 9>/usr/src/fission/.build.lock
flock 9

if [ -f "${SRC_PKG}/Cargo.toml" ]; then
    # --- Project mode: the source package is a Cargo project ---
    cd "${SRC_PKG}"
    cargo build --release
    bins=$(cargo metadata --no-deps --format-version 1 |
        jq -r '.packages[].targets[] | select(.kind | index("bin")) | .name')
    count=$(echo "${bins}" | grep -c . || true)
    if [ "${count}" -eq 0 ]; then
        echo "Error: no [[bin]] targets in ${SRC_PKG}/Cargo.toml"
        exit 1
    elif [ "${count}" -eq 1 ]; then
        # Single binary: deploy under the default name the runtime expects.
        cp "${CARGO_TARGET_DIR}/release/${bins}" "${DEPLOY_PKG}/handler"
    else
        # Multiple binaries: deploy all; the function's --entrypoint
        # selects one at specialize time.
        for bin in ${bins}; do
            cp "${CARGO_TARGET_DIR}/release/${bin}" "${DEPLOY_PKG}/${bin}"
        done
    fi
else
    # --- Single-file mode: bare .rs file(s) wrapped in the template crate ---
    template_src=/usr/src/fission/template/src
    find "${template_src}" -name '*.rs' ! -name main.rs -delete
    if [ -f "${SRC_PKG}" ]; then
        cp "${SRC_PKG}" "${template_src}/handler.rs"
    elif [ -f "${SRC_PKG}/handler.rs" ]; then
        # handler.rs plus optional extra modules
        cp "${SRC_PKG}"/*.rs "${template_src}/"
    else
        rs_files=("${SRC_PKG}"/*.rs)
        if [ ${#rs_files[@]} -ne 1 ] || [ ! -f "${rs_files[0]}" ]; then
            echo "Error: single-file packages need exactly one .rs file" \
                "or a handler.rs; use a Cargo project for anything bigger"
            exit 1
        fi
        cp "${rs_files[0]}" "${template_src}/handler.rs"
    fi
    cd /usr/src/fission/template
    # Dependencies are pre-fetched in the image; stay offline if possible.
    cargo build --release --offline || cargo build --release
    cp "${CARGO_TARGET_DIR}/release/handler" "${DEPLOY_PKG}/handler"
fi

chmod 0755 "${DEPLOY_PKG}"/*
echo "Build output:"
ls -l "${DEPLOY_PKG}"
