for file in $(find . -name envconfig.json); do
    jq -s <$file >$file.sorted
    mv $file.sorted $file
done
