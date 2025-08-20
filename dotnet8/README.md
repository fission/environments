# Fission: dotnet 8.0 C# Environment

This is a simple dotnet core 8.0 C# environment for Fission.

It's a Docker image containing the dotnet 8.0.0 runtime. The image 
uses Kestrel with ASP.NET Core to host the internal web server and uses 
Roslyn to compile the uploaded code.

The image supports compiling and running code with types defined in
mscorlib and does not at present support other library references.
One workaround for this would be to add the references to this project's
.csproj file and rebuild the container.

The environment works via convention where you create a C# class
containing a method named Execute taking a single
parameter, a FissionContext object.

The FissionContext object gives access to the arguments and other items 
like logging. Please see FissionContext.cs for public API.

Example of simplest possible class to be executed:

```
using System;
using Fission.DotNet.Common;

public class MyFunction {
    public string Execute(FissionContext context) {
        return null;
    }
}
```

Please see examples below. Ready-to-run examples are available in the examples directory:
- **HelloWorld** - Basic function demonstration
- **HttpTriggerExample** - HTTP request handling with context detection
- **AsyncFunctionExample** - Demonstrates async/await patterns
- **MultiFileExample** - Complex multi-file project with MVC pattern

## Rebuilding and pushing the image

To rebuild the image you will have to install Docker with version higher than 17.05+
in order to support multi-stage builds feature.  

### Rebuild containers

Move to the directory containing the source and start the container build process:

```
docker build -t USER/dotnet8-env .
```

After the build finishes push the new image to a Docker registry using the 
standard procedure.

## Echo example

### Setup fission environment
First you need to setup the fission according to your cluster setup as 
specified here: https://github.com/fission/fission


### Create the class to run

Secondly you need to create a file /tmp/func.cs containing the following code:

```
using System;
using Fission.DotNet.Common;

public class MyFunction 
{
    public string Execute(FissionContext context){
        context.Logger.WriteInfo("executing.. {0}", context.Arguments["text"]);
        return (string)context.Arguments["text"];
    }
}
``` 
### Run the example

Lastly to run the example:

```
$ fission env create --name dotnet8 --image fission/dotnet8-env

$ fission function create --name echo --env dotnet8 --code /tmp/func.cs

$ fission route create --method GET --url /echo --function echo

$ curl http://$FISSION_ROUTER/echo?text=hello%20world!
  hello world
```

## Addition service example

### Setup fission environment
First you need to setup the fission according to your cluster setup as 
specified here: https://github.com/fission/fission


### Create the class to run

Secondly you need to create a file /tmp/func.cs containing the following code:

```
using System;
using Fission.DotNet.Common;

public class MyFunction 
{
    public string Execute(FissionContext context){
        var x = Convert.ToInt32(context.Arguments["x"]);
        var y = Convert.ToInt32(context.Arguments["y"]);
        return (x+y).ToString();
    }
}
``` 
### Run the example

Lastly to run the example:

```
$ fission env create --name dotnet8 --image fission/dotnet8-env

$ fission function create --name addition --env dotnet8 --code /tmp/func.cs

$ fission route create --method GET --url /add --function addition

$ curl "http://$FISSION_ROUTER/add?x=30&y=12"
  42
```

## Accessing http request information example

### Setup fission environment
First you need to setup the fission according to your cluster setup as 
specified here: https://github.com/fission/fission


### Create the class to run

Secondly you need to create a file /tmp/func.cs containing the following code:

```
using System;
using Fission.DotNet.Common;

public class MyFunction
{
    public string Execute(FissionContext context){
        var httpContext = context as FissionHttpContext;
        if (httpContext != null) {
            var buffer = new System.Text.StringBuilder();
            foreach(var header in httpContext.Headers){
                    buffer.AppendLine(header.Key);
                    foreach(var item in header.Value){
                            buffer.AppendLine($"\t{item}");
                    }
            }
            buffer.AppendLine($"Url: {httpContext.Url}, method: {httpContext.Method}");
            return buffer.ToString();
        }
        return "Not an HTTP request";
    }
}

``` 
### Run the example

Lastly to run the example:

```
$ fission env create --name dotnet8 --image fission/dotnet8-env

$ fission function create --name httpinfo --env dotnet8 --code /tmp/func.cs

$ fission route create --method GET --url /http_info --function httpinfo

$ curl "http://$FISSION_ROUTER/http_info"
Accept
	*/*;q=1
Host
	fissionserver:8888
User-Agent
	curl/7.47.0
Url: http://fissionserver:8888, method: GET

```

## Accessing http request body example

### Setup fission environment
First you need to setup the fission according to your cluster setup as 
specified here: https://github.com/fission/fission


### Create the class to run

Secondly you need to create a file /tmp/func.cs containing the following code:

```
using System.IO;
using System.Text.Json;
using Fission.DotNet.Common;

public class MyFunction
{
    public string Execute(FissionContext context)
    {
        var httpContext = context as FissionHttpContext;
        if (httpContext != null && httpContext.Body != null) {
            var person = JsonSerializer.Deserialize<Person>(httpContext.Body);
            return $"Hello, my name is {person.Name} and I am {person.Age} years old.";
        }
        return "No body found";
    }
}

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

``` 
### Run the example

Lastly to run the example:

```
$ fission env create --name dotnet8 --image fission/dotnet8-env

$ fission function create --name httpbody --env dotnet8 --code /tmp/func.cs

$ fission route create --method POST --url /http_body --function httpbody

$ curl -XPOST "http://$FISSION_ROUTER/http_body" -d '{ "Name":"Arthur", "Age":42}'
Hello, my name is Arthur and I am 42 years old.

```

## Builder Support

The dotnet8 environment includes a builder image for compiling source packages with NuGet dependencies.

### Create the environment with builder

```
$ fission env create --name dotnet8 \
    --image fission/dotnet8-env \
    --builder fission/dotnet8-builder
```

### Deploy a source package

Create a project structure:
```
MyProject/
├── MyProject.csproj
├── MyFunction.cs
└── nuget.txt (optional - for additional NuGet packages)
```

Example MyProject.csproj:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="*.cs" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include="Fission.DotNet.Common">
      <HintPath>../Fission.DotNet.Common.dll</HintPath>
    </Reference>
  </ItemGroup>
</Project>
```

Create a zip package and deploy:
```
$ zip -r myproject.zip MyProject/
$ fission fn create --name myproject --env dotnet8 --src myproject.zip
```

## Async Function Support

The environment supports async/await patterns:

```
using System.Threading.Tasks;
using Fission.DotNet.Common;

public class MyFunction
{
    public async Task<object> Execute(FissionContext context)
    {
        // Simulate async work
        await Task.Delay(100);
        
        // Async HTTP calls, database operations, etc.
        return "Async result";
    }
}
```

## Logging Support

Functions can use structured logging via ILogger:

```
using Microsoft.Extensions.Logging;
using Fission.DotNet.Common;

public class MyFunction
{
    public object Execute(FissionContext context, ILogger logger)
    {
        logger.LogInformation("Processing request");
        
        try
        {
            // Function logic
            return "Success";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing request");
            throw;
        }
    }
}
```

## CORS Support

Enable CORS for cross-origin requests:

```
using Fission.DotNet.Common;

public class MyFunction
{
    public object Execute(FissionContext context)
    {
        // Your function logic
        return "API Response";
    }

    public static void SetCorsPolicy(ICorsPolicy policy)
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowAnyOrigin();
        policy.AllowCredentials();
    }
}
```


## Developing/debugging the environment locally

The easiest way to debug the environment is to open the directory in
Visual Studio Code (VSCode) as that will setup debugger for you the
first time.

Remember to install the excellent extension 
"C# for Visual Studio Code (powered by OmniSharp)" to get statement completion

To debug locally:
1. Open the directory in VSCode. 
This will prompt restore of packages and query if debugger setup is needed. Accept both prompts.
2. Press F5 to start the web server. Set breakpoints etc.
3. Add a code file containing valid C# at /tmp/func.cs 
4. Specialize the service with curl via post
```
$ curl -XPOST http://localhost:8888/specialize
```
5. Call your function with curl
```
$ curl -XGET http://localhost:8888
``` 

## Additional Features  

**1. Namespace support:**

You can use namespaces for your function classes. The main execution class can have any name, and the entry method is Execute:

```
using System;
using Fission.DotNet.Common;

namespace MyNamespace
{
    public class MyFunction 
    {
        public string Execute(FissionContext context){
            var helper = new HelperClass();
            return helper.Process(context);
        }
    }
    
    public class HelperClass
    {
        public string Process(FissionContext context){
            // Helper logic
            return "Processed";
        }
    }
}
```

**2. Configuration file support:**  	
			
With Fission builder support, you can include additional configuration files in your source package that can be read by your function.

Example package structure:
```
Source Package zip:
--source.zip
   |--MyFunction.cs
   |--MyProject.csproj
   |--appsettings.json
   |--nuget.txt (optional)
```

Example appsettings.json:
```json
{ 
    "name": "Alpha",
    "endpoints": [
        { "port": 1002 },
        { "port": 3004 } 
    ]
}
```

Example function using configuration:
``` 
using System;
using System.IO;
using System.Text.Json;
using Fission.DotNet.Common;
			
public class MyFunction
{
    public string Execute(FissionContext context){
        // Read configuration file
        var configPath = Path.Combine(context.ExecutionPath, "appsettings.json");
        if (File.Exists(configPath)) {
            var json = File.ReadAllText(configPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return $"Endpoint port: {settings.Endpoints[0].Port}";
        }
        return "No config found";
    }
}

public class AppSettings
{
    public string Name { get; set; }
    public List<Endpoint> Endpoints { get; set; }
}

public class Endpoint
{
    public int Port { get; set; }
}
```

**3. NuGet support:**

With the builder environment, you can add NuGet packages to your functions. Create a nuget.txt file in your source package listing the packages:

```
Newtonsoft.Json
Dapper
Serilog
```

These packages will be automatically restored during the build process.

## Troubleshooting

### Common Issues

**Build Failures:**
- Ensure .csproj file has correct format
- Check that Fission.DotNet.Common reference path is correct
- Verify all .cs files are included with `<Compile Include="*.cs" />`

**Function Timeouts:**
- Check function logs: `fission fn logs --name function-name`
- Verify package build status: `fission pkg list`
- Check build logs: `fission pkg info --name package-name`

**HTTP Context Issues:**
- Cast context appropriately: `context as FissionHttpContext`
- Check if X-Forwarded-Proto header is set for HTTP detection

### Debug Commands

```bash
# Check environment status
fission env list

# Check package build status  
fission pkg list

# View package build logs
fission pkg info --name package-name

# View function logs
fission fn logs --name function-name

# Test function directly
fission fn test --name function-name
```