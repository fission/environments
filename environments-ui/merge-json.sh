
#!/bin/bash

echo "[" > temp.json

for FILE in *; 
do 
    if [ -f "$FILE/envconfig.json" ]; then
     ( cd "$FILE" &&  cat envconfig.json) >> temp.json
     echo "," >> temp.json
    fi
done 
sed '$s/,$//' temp.json > ./environments-ui/src/resources/environments.json
echo "]" >> ./environments-ui/src/resources/environments.json
rm -rf temp.json
cd environments-ui && npm i && npm run build