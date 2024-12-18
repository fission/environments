#!/bin/sh
cd ${SRC_PKG}

if [[ -n "$NPM_TOKEN" ]] && [[ -n "$NPM_REGISTRY" ]]; then
    touch bunfig.toml
    echo "[install.scopes]" >> bunfig.toml
    echo "\"@private\" = { token = \"$NPM_TOKEN\", url = \"$NPM_REGISTRY\" }" >> bunfig.toml
fi

bun install && cp -r ${SRC_PKG} ${DEPLOY_PKG}
