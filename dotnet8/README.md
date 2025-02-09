# Fission .NET Environment

This project is an environment for [Fission](https://fission.io/), a serverless plugin for Kubernetes. It is based on Fission's official .NET environment but introduces support for more recent versions of the .NET framework, as the original version is still limited to .NET Core 2.0. This environment is specifically designed for **.NET 8 on Linux**.

## Main Features

- **Support for newer versions of .NET**: This environment is designed to work with **.NET 8 on Linux**, offering developers an updated and improved experience compared to the official environment.
  
- **Multi-assembly project management**: One of the key features of this environment is the ability to handle and run projects composed of multiple linked assemblies. The assemblies can be compressed into a ZIP file, simplifying the process of distribution and integration.

## Inspiration

This project is inspired by Fission's official environment for .NET Core 2.0 but focuses on improvements and updates requested by the community, allowing developers to work with newer versions of the .NET framework and complex projects that include multiple assemblies.

## Usage

1. **Add the environment to Fission**:
   To add this .NET 8 environment to Fission, use the following command:

   ```bash
   fission env create --name dotnet8 --image <your_custom_image>
   ```
    
2. **Project creation**: 
   - Create a **class library project** in .NET.
   - Add the **Fission NuGet package** to your project.
   - Create a class with the following function:
     
     ```csharp
     public object Execute(FissionContext input)
     {
         return "Hello World";
     }
     ```

3. **Compression**: Compress the assemblies and related files into a ZIP file.

4. **Deploy to Fission**: Use this environment to deploy your project to Fission, leveraging the ability to handle multiple linked assemblies. After compressing your project into a ZIP file, you can create the function in Fission with the following command:

    ```bash
    fission fn create --name <function_name> --env dotnet8 --code <your_project.zip> --entrypoint <name_of_assembly_without_extension>:<namespace>:<classname>
    ```
    Replace `<function_name>` with the name of your function, and `<your_project.zip>` with the path to your ZIP file.

### HTTP trigger

#### C# Example

```csharp
public class MyFunction
{
    public object Execute(FissionContext context)
    {
        if (context is FissionHttpContext request)
        {
            return $"Hello from HTTP trigger! Method: {request.Method}, URL: {request.Url}";
        }
        return "Invalid context";
    }
}
```

#### CORS

To make external calls, you need to enable CORS (Cross-Origin Resource Sharing). This allows your function to handle requests from different origins. For each HTTP trigger, you will need to create an additional trigger of type OPTIONS.

#### C# Example

```csharp
public class MyFunction
{
    public object Execute(FissionContext context)
    {
        if (context is FissionHttpContext request)
        {
            return $"Hello from HTTP trigger! Method: {request.Method}, URL: {request.Url}";
        }
        return "Invalid context";
    }

    public static void SetCorsPolicy(ICorsPolicy policy)
    {
        policy.AllowAnyHeader();
        policy.AllowAnyMethod();
        policy.AllowCredentials();
        policy.AllowAnyOrigin();      
    }
}
```

### Cron trigger

#### C# Example

```csharp
public class MyFunction
{
    public object Execute(FissionContext context)
    {
        return "Hello from Cron trigger!";
    }
}
```

### Queue trigger

#### C# Example

```csharp
public class MyFunction
{
    public object Execute(FissionContext context)
    {
        if (context is FissionMqContext request)
        {
            return $"Hello from Queue trigger! Topic: {request.Topic}";
        }
        return "Invalid context";
    }
}
```

### Async/Await

You can use asynchronous methods in your function by utilizing the `async` and `await` keywords. This allows you to perform asynchronous operations, such as I/O-bound tasks, without blocking the main thread.

#### C# Example

```csharp
public class MyFunction
{
    public async Task<object> ExecuteAsync(FissionContext context)
    {
        await Task.Delay(1000); // Simulate an asynchronous operation
        return "Hello from async function!";
    }
}
```

### Logging

You can integrate logging into your function by using the `ILogger` interface provided by the `Fission.DotNet.Common` namespace. This allows you to log information, warnings, errors, and other messages. Below is an example of how logging is implemented in a class.

#### C# Example

```csharp
using Fission.DotNet.Common;

public class MyFunction
{
    public async Task<string> Execute(FissionContext input, ILogger logger)
    {
        logger.LogInformation("Function execution started.");
        // ...function logic...
        logger.LogInformation("Function execution completed.");
        return "Hello with logging!";
    }
}
```

### Service collection

You can use service registration to manage dependencies and create the function class via Inversion of Control (IoC). This allows you to inject services into your function class, making it easier to manage dependencies and improve testability.

#### C# Example

```csharp
using Fission.DotNet.Common;
using Microsoft.Extensions.DependencyInjection;

public class MyFunction
{
    private readonly IService service;

    public MyFunction(IService service)
    {
        this.service = service;
    }
    public async Task<string> Execute(FissionContext input, ILogger logger)
    {
        return await service.Execute(input, logger);
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IService, Service>();
    }
}

```

## Requirements

- Kubernetes cluster
- Fission installed
- .NET SDK 8 for Linux

## Related NuGet Package

This project uses the **[Fission.DotNet.Common](https://www.nuget.org/packages/Fission.DotNet.Common/)** package. It provides essential functionality for interacting with the Fission environment in .NET, making it easier to work with serverless functions.
