# Fission: .NET 6.0 C# Environment Builder

This is a .NET 6.0 C# environment builder for Fission. It supports building multi-file projects with NuGet dependencies.

During build, it also does a pre-compilation to prevent any compilation issues during function environment pod specialization.
Thus we get the function compilation issues during builder phase in package info's build logs itself.

Once the build is finished, the output package (deploy archive) will be uploaded to storagesvc to store.
Then, during the specialization, the fetcher inside function pod will fetch the package from storagesvc for function loading and will call on the  **/v2/specialize** endpoint of fission environment with required parameters.

There further environment will compile it and execute the function.

## Examples

See `/examples/builder-example` for a complete example.

Example of simplest possible class to be executed:

The source package structure in zip file :

```
 Source Package zip :
 --soruce.zip
	|--func.cs
	|--nuget.txt
	|--exclude.txt
	|--....MiscFiles(optional)
	|--....MiscFiles(optional)
```

**func.cs** --> This contains original function body with Executing method name as : Execute

 
**nuget.txt**--> this file contains list of nuget packages required by your function , in this file put one line per nuget with nugetpackage name:version(optional) format, for example :

```
RestSharp
CsvHelper
Newtonsoft.json:10.2.1.0
```
  
 **exclude.txt**--> list of dlls that will be excluded from compilation
 
```
Newtonsoft.json:Newtonsoft.json.dll
```

## Usage

```
fission env list
fission fn list
 ```
 Create Environment with builder, supposing that the builder image name is `fission/dotnet6-builder` and hosted on dockerhub as `fission/dotnet6-builder`
 ```
fission environment create --name dotnetwithnuget --image fission/dotnet6  --builder  fission/dotnet6-builder
 ```
 Verify fission-builder and fission-function namespace for new pods (pods name beginning with env name which we have given like *dotnetwithnuget-xxx-xxx*)
 ```
kubectl get pods -n fission-builder
kubectl get pods -n fission-function
 ```
Create a package from source zip using this environment name
 ```
fission package create --src funccsv.zip --env dotnetwithnuget
 ```
Check package status
 ```
fission package info --name funccsv-zip-xyz
```

#Status of package should be *failed / running / succeeded* .
Wait if the status is running, until it fails or succeeded. For detailed build logs, you can shell into builder pod in fission-builder namespace and verify log location mentioned in above command's result output.

Now If the result is succeeded , then go ahead and create function using this package.

*--entrypoint* flag is optional if your function body file name is func.cs  (which it should be as builder need that), else put the filename (without extension)
 ```
 fission fn create --name dotnetcsvtest --pkg funccsv-zip-xyz --env dotnetcorewithnuget --entrypoint "func"
 ```
Test the function execution:
``` 
 fission fn test --name dotnetcsvtest
```
Above would execute the function and will output the enum value as written in dll.