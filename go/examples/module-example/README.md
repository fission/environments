# Go module usage

1. Initialize your project

    ```console
    go mod init
    ```

2. Add dependencies
    See [modules daily-workflow](https://github.com/golang/go/wiki/Modules#daily-workflow)

3. Verify

    ```console
    go mod tidy
    go mod verify
    ```

4. Archive and create package as usual

    ```console
    $ zip -r go.zip .
        adding: go.mod (deflated 26%)
        adding: go.sum (deflated 1%)
        adding: README.md (deflated 37%)
        adding: main.go (deflated 30%)
        
    $ fission pkg create --env go --src go.zip
    ```
