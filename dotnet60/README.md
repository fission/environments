# fission-dotnet6
A .NET 6 function environment for [Fission](https://fission.io/).

The environment Docker image (_fission-dotnet6_) contains the .NET 6.0 runtime and uses an ASP.NET Core web api application to make the relevant endpoints available to Fission. This image supports compiling and running single-file functions using the types available in the core .NET 6 assemblies. To use multi-file projects and download packages with NuGet, use builder image in it's directory. 

The environment works via the IFissionFunction interface (provided by the _Fission.Functions_ assembly.) A function for _fission-dotnet6_ is presented as a class which implements the _IFissionFunction_ interface, thus:

```
using System;
using Fission.Functions;

public class HelloWorld : IFissionFunction
{
    public object Execute(FissionContext context)
    {
        return "hello, world!";
    }
}

```

Logging and access to function call parameters are accessible through the _context_ parameter. Please see the inline documentation for _FissionContext.cs_ and the examples for further details.

## Rebuilding the image

To rebuild the containers, in the top-level directory, execute the following:

```
docker build . --platform=linux/amd64,linux/arm64,linux/arm -t repository/fission-dotnet6:dev --push
```

## Setting up the Fission environment

To set up the .NET 6 Fission environment, execute the command:

```
fission env create --name dotnet6 --image repository/fission-dotnet6:dev --version 1
```

## Configuring and testing a function

If the above function is contained within the file `HelloWorld.cs` (see the examples directory), and the environment has been set up as above, then it can be installed as follows:

```
fission fn create --name hello-dotnet --env dotnet6 --code HelloWorld.cs
```

And tested thus:

```
fission fn test --name hello-dotnet
```

