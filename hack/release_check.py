#!/usr/bin/python3

import json
import os
import sys
import requests

GHCR_ORG = "fission"

GHCR_TOKEN_URL = "https://ghcr.io/token?scope=repository:{org}/{env}:pull"
GHCR_URL = "https://ghcr.io/v2/{org}/{env}/tags/list"
def check_if_image_exists(image,tag):
    # GHCR v2 API needs a bearer token even for public images;
    # an anonymous token is sufficient for pull scope.
    token_resp = requests.get(GHCR_TOKEN_URL.format(org=GHCR_ORG, env=image), timeout=30)
    token_resp.raise_for_status()
    token = token_resp.json().get("token")
    if not token:
        raise RuntimeError(f"GHCR token response missing 'token' for image '{image}': {token_resp.text[:200]}")
    headers = {"Authorization": f"Bearer {token}"}
    resp = requests.get(GHCR_URL.format(org=GHCR_ORG, env=image), headers=headers, timeout=30)
    if resp.status_code == 200:
        return tag in resp.json().get("tags", [])
    if resp.status_code == 404:
        # Repository does not exist yet -> release needed.
        return False
    # Auth/rate-limit/5xx errors must fail the job (fail-closed) rather
    # than masquerade as "image absent" and trigger spurious re-releases.
    resp.raise_for_status()
    raise RuntimeError(f"Unexpected GHCR response for image '{image}': {resp.status_code} {resp.text[:200]}")

def set_output(name, value):
    output_path = os.environ.get("GITHUB_OUTPUT")
    if not output_path:
        # Local/dry-run fallback: print instead of failing with a KeyError.
        print(f"{name}={value}")
        return
    with open(output_path, "a") as f:
        f.write(f"{name}={value}\n")

def main(package_list_str):
    package_list = package_list_str.strip("[]")
    package_list = [p.strip().strip('"') for p in package_list.split(',') if p.strip()]

    print(package_list, type(package_list))
    versions_to_be_released = []
    for package in package_list:
        with open(f"{package}/envconfig.json") as f:
            env_config_json = json.load(f)
            for env_config in env_config_json:
                if not check_if_image_exists(env_config['image'],env_config['version']):
                    versions_to_be_released.append(
                        {
                            "image": env_config['image'],
                            "tag": env_config['version'],
                            "env": package,
                            "builder": env_config.get("builder",""),
                        }
                    )
    json_output = {
        "include": versions_to_be_released,
    }
    print(json_output)
    release_needed = True if len(versions_to_be_released) > 0 else False
    set_output("versions_to_be_released", json.dumps(json_output))
    # release.yaml gates on `release_needed == 'true'`.
    set_output("release_needed", "true" if release_needed else "false")

if __name__ == "__main__":
   main(sys.argv[1])
