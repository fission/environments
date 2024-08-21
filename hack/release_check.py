#!/usr/bin/python3

import json
import sys
import requests
import subprocess

FISSION_REPO = "ghcr.io/fission"

GHCR_URL = "https://ghcr.io/v2/{repo}/{env}/tags/{tag}"
def check_if_image_exists(image,tag):
    docker_uri = GHCR_URL.format(
        repo=FISSION_REPO,
        env=image,
        tag=tag,
    )
    resp = requests.get(docker_uri)
    json_resp = resp.json()
    if "message" in json_resp and "not found" in json_resp['message']:
        return False
    elif "images" in json_resp and len(json_resp["images"]) > 0:
        return True
    else:
        return False

def main(package_list_str):
    package_list = package_list_str[1:len(package_list_str)-1]
    package_list = package_list.split(',')

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
    subprocess.run(["echo", f"::set-output name=versions_to_be_released::{json.dumps(json_output)}"])
    subprocess.run(["echo", f"::set-output name=release_needed::{json.dumps(release_needed)}"])

if __name__ == "__main__":
   main(sys.argv[1])


