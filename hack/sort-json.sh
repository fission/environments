for file in $(find . -name envconfig.json); do
    jq -S <$file >$file.sorted
    mv $file.sorted $file
done
