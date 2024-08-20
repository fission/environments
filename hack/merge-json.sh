#!/bin/bash

TMP_FILE=temp.json
JQ=jq

if ! command -v sed >/dev/null 2>&1; then
    echo "You must install 'sed' to use this script."
    exit 1
fi
if ! command -v $JQ >/dev/null; then
    echo "jq is not installed. Please install it and try again."
    exit 1
fi

echo "[" >"$TMP_FILE"
for file in $(find . -name envconfig.json); do
    echo "Validating $file"
    if ! $JQ . <"$file" >/dev/null; then
        echo "Invaild json file $file. Please check $file"
        echo "JSON merge failed. Please try again."
        exit 1
    fi
    echo "Merging $file"
    cat "$file" >>"$TMP_FILE"
    echo "," >>"$TMP_FILE"
done
sed -ie '$s/,$//' "$TMP_FILE"
echo "]" >>"$TMP_FILE"

if ! $JQ . <"$TMP_FILE" >/dev/null; then
    echo "JSON merge failed. Please check the merge result $TMP_FILE and try again."
    exit 1
fi

echo "Updating environments.json"
cp -v $TMP_FILE ./environments.json
rm $TMP_FILE*

# cd environments-ui && npm i && npm run build
