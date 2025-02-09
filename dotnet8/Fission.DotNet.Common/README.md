# Fission.DotNet.Common

This project is a common library for the .NET environment of [Fission](https://fission.io/), a serverless plugin for Kubernetes. The library provides common functionalities and utilities to facilitate the development of serverless functions with Fission on .NET.

## Main Features

- **Common Utilities**: Provides a set of common utilities to simplify the development of serverless functions.
- **.NET 8 Compatibility**: Designed to work with **.NET 8 on Linux**, offering an updated and improved experience.
- **Multi-Assembly Management**: Supports projects composed of multiple linked assemblies, simplifying the deployment and integration process.

## Inspiration

This project is inspired by the official Fission environment for .NET Core 2.0 but focuses on improvements and updates requested by the community, allowing developers to work with newer versions of the .NET framework and complex projects that include multiple assemblies.

## Usage

1. **Add the library to your project**:
   Add the NuGet package `Fission.DotNet.Common` to your .NET project.

   ```bash
   dotnet add package Fission.DotNet.Common

2. **Create the project**:
- Create a **class library project** in .NET.
- Add the NuGet package `Fission.DotNet.Common` to your project.
- Create a class with the following function:

    ```csharp
    using Fission.DotNet.Common;

    public class MyFunction
    {
        public object Execute(FissionContext input)
        {
            return "Hello World";
        }
    }
     ```
3. **Compression**: Compress the assemblies and related files into a ZIP file.

4. **Deploy to Fission**: Use this library to deploy your project to Fission, leveraging the ability to manage multiple linked assemblies. After compressing your project into a ZIP file, you can create the function in Fission with the following command:

    ```bash
    fission fn create --name <function_name> --env dotnet8 --code <your_project.zip> --entrypoint <name_of_assembly_without_extension>:<namespace>:<classname>
    ```
    Replace `<function_name>` with the name of your function and `<your_project.zip>` with the path to your ZIP file.     