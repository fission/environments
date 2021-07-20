
#!/bin/bash
JQ=jq
for file in $(find . -name envconfig.json); do
    $JQ . -S <$file >$file.sorted
    mv $file.sorted $file
done
