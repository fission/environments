using System;
using System.Diagnostics;
using System.IO.Compression;
using Fission.DotNet.Interfaces;
using Fission.DotNet.Model;

namespace Fission.DotNet.Services;

public class SpecializeService : ISpecializeService
{
    private readonly ILogger<SpecializeService> _logger;
    private readonly IFunctionStoreService _functionStoreService;

    public SpecializeService(ILogger<SpecializeService> logger, IFunctionStoreService functionStoreService)
    {
        this._logger = logger;
        this._functionStoreService = functionStoreService;
    }

    public void Specialize(FissionSpecializeRequest request)
    {
        if (request == null)
        {
            throw new NullReferenceException("Request is null");
        }

        if (System.IO.File.Exists("/userfunc/deployarchive.tmp"))
        {
            _logger.LogInformation("Deploy archive exists");
            UnzipDeployArchive("/userfunc/deployarchive.tmp", "/function");
        }
        else
        {
            if (System.IO.File.Exists("/userfunc/deployarchive"))
            {
                _logger.LogInformation("Deploy archive exists");
                UnzipDeployArchive("/userfunc/deployarchive", "/function");
            }
            else
            {
                _logger.LogInformation("Deploy archive does not exist");
                throw new Exception("Deploy archive does not exist");
            }
        }

        // Check if this is a builder-compiled deployment by looking for pre-built application DLLs
        var dllFiles = Directory.GetFiles("/function", "*.dll", SearchOption.AllDirectories);
        
        // Filter out dependency DLLs to find actual application DLLs
        var knownDependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Fission.DotNet.Common.dll",
            "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
            "Newtonsoft.Json.dll",
            "System.Text.Json.dll",
            "Microsoft.OpenApi.dll"
        };
        
        // Look for DLLs that match common patterns for function assemblies
        var applicationDlls = dllFiles.Where(dll => 
        {
            var fileName = Path.GetFileName(dll);
            // Skip known dependencies
            if (knownDependencies.Contains(fileName))
                return false;
            // Skip system libraries
            if (fileName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                fileName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }).ToArray();
            
        var hasPreBuiltDlls = applicationDlls.Length > 0;
        
        _logger.LogInformation("Found {DllCount} total DLL files, {AppDllCount} application DLLs, has pre-built DLLs: {HasPreBuilt}", 
            dllFiles.Length, applicationDlls.Length, hasPreBuiltDlls);
        
        if (hasPreBuiltDlls)
        {
            // This is a builder-compiled deployment - use the pre-built DLL
            // If we have multiple candidates, prefer one that matches the function name or contains "Example"
            var mainDll = applicationDlls.FirstOrDefault(dll => 
                Path.GetFileNameWithoutExtension(dll).Contains("Example", StringComparison.OrdinalIgnoreCase) ||
                Path.GetFileNameWithoutExtension(dll).Equals(request.functionName, StringComparison.OrdinalIgnoreCase))
                ?? applicationDlls.FirstOrDefault();
            if (mainDll != null)
            {
                var assemblyName = Path.GetFileName(mainDll);
                var functionName = string.IsNullOrEmpty(request.functionName) ? "MyFunction" : request.functionName;
                
                _logger.LogInformation("Using pre-built assembly: {AssemblyName} with function: {FunctionName}", assemblyName, functionName);
                
                // For builder deployments, the function identifier format is: assemblyFileName:namespace:functionName
                // Try to detect namespace from assembly name (e.g., MultiFileExample.dll -> MultiFileExample namespace)
                var assemblyBaseName = Path.GetFileNameWithoutExtension(assemblyName);
                var nameSpace = "";
                
                // If the assembly name suggests it has a namespace (e.g., MultiFileExample), use it
                if (assemblyBaseName.Contains("Example") || assemblyBaseName.Contains("File"))
                {
                    nameSpace = assemblyBaseName;
                }
                
                var functionIdentifier = $"{assemblyBaseName}:{nameSpace}:{functionName}";
                _logger.LogInformation("Function identifier: {FunctionIdentifier}", functionIdentifier);
                var functionStore = new FunctionStore(functionIdentifier);
                _functionStoreService.SetFunction(functionStore);
            }
            else
            {
                throw new Exception("No main assembly found in pre-built deployment");
            }
        }
        else
        {
            // This is a source code deployment - compile it
            var functionName = request.functionName;
            if (string.IsNullOrEmpty(functionName))
            {
                // Try to determine function name from the extracted files
                var csFiles = Directory.GetFiles("/function", "*.cs", SearchOption.AllDirectories);
                if (csFiles.Length > 0)
                {
                    // Use the first .cs file name as the function name
                    functionName = Path.GetFileNameWithoutExtension(csFiles[0]);
                    _logger.LogInformation("Using function name from file: {FunctionName}", functionName);
                }
                else
                {
                    // Default function name for single file deployments
                    functionName = "MyFunction";
                    _logger.LogInformation("Using default function name: {FunctionName}", functionName);
                }
            }

            // For source code deployments, construct the function identifier and compile
            var functionIdentifier = $"SingleFileProject:{functionName}:{functionName}";
            var functionStore = new FunctionStore(functionIdentifier);
            
            // Check if we need to compile source code
            if (HasSourceCode(functionStore.Assembly))
            {
                _logger.LogInformation("Source code detected, compiling...");
                CompileSourceCode(functionStore.Assembly);
            }
            
            _functionStoreService.SetFunction(functionStore);
        }
    }

    private void UnzipDeployArchive(string zipFilePath, string extractPath)
    {
        if (System.IO.File.Exists(zipFilePath))
        {
            _logger.LogInformation("Deploy archive exists at {ZipFilePath}", zipFilePath);
            
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            try
            {
                // Try to unzip the ZIP file into the /function folder
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                _logger.LogInformation("Deploy archive unzipped to {ExtractPath}", extractPath);
            }
            catch (System.IO.InvalidDataException ex)
            {
                _logger.LogWarning("ZIP file is corrupted: {Error}. Attempting to handle as single file.", ex.Message);
                
                // Handle corrupted ZIP by treating it as a single file
                HandleCorruptedZipAsSingleFile(zipFilePath, extractPath);
            }
        }
        else
        {
            _logger.LogWarning("Deploy archive does not exist at {ZipFilePath}", zipFilePath);
        }
    }

    private void HandleCorruptedZipAsSingleFile(string zipFilePath, string extractPath)
    {
        try
        {
            // Try to read the file as a single .cs file
            var fileContent = System.IO.File.ReadAllText(zipFilePath);
            
            // Check if it looks like C# code
            if (fileContent.Contains("using") || fileContent.Contains("class") || fileContent.Contains("public"))
            {
                _logger.LogInformation("File appears to be C# code, treating as single file");
                
                // Create a simple function file
                var functionPath = Path.Combine(extractPath, "MyFunction.cs");
                System.IO.File.WriteAllText(functionPath, fileContent);
                
                // Create a minimal .csproj file using ProjectReference (like the examples)
                var csprojContent = @"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""/app/Fission.DotNet.Common/Fission.DotNet.Common.csproj"" />
  </ItemGroup>
</Project>";
                
                var csprojPath = Path.Combine(extractPath, "SingleFileProject.csproj");
                System.IO.File.WriteAllText(csprojPath, csprojContent);
                
                _logger.LogInformation("Created single file project structure in {ExtractPath}", extractPath);
            }
            else
            {
                _logger.LogError("File does not appear to be valid C# code");
                throw new Exception("Invalid file content");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle corrupted ZIP as single file");
            throw;
        }
    }

    private bool HasSourceCode(string assemblyName)
    {
        // Check if the DLL exists, if not, check for .csproj or .cs files
        var dllPath = $"/function/{assemblyName}";
        if (System.IO.File.Exists(dllPath))
        {
            _logger.LogInformation("DLL already exists at {DllPath}", dllPath);
            return false; // DLL exists, no need to compile
        }

        // Look for project files or source files
        var projectFiles = Directory.GetFiles("/function", "*.csproj", SearchOption.AllDirectories);
        var sourceFiles = Directory.GetFiles("/function", "*.cs", SearchOption.AllDirectories);
        
        _logger.LogInformation("Found {ProjectFileCount} .csproj files and {SourceFileCount} .cs files", 
            projectFiles.Length, sourceFiles.Length);
            
        return projectFiles.Length > 0 || sourceFiles.Length > 0;
    }

    private void CompileSourceCode(string expectedAssemblyName)
    {
        try
        {
            // Copy required libraries to function directory
            CopyRequiredLibraries();
            
            // Look for .csproj files
            var projectFiles = Directory.GetFiles("/function", "*.csproj", SearchOption.AllDirectories);
            
            if (projectFiles.Length > 0)
            {
                var projectFile = projectFiles[0]; // Use the first project file found
                var projectDir = Path.GetDirectoryName(projectFile);
                
                _logger.LogInformation("Compiling project: {ProjectFile}", projectFile);
                
                // Run dotnet publish to get all dependencies (instead of build)
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"publish \"{projectFile}\" -c Release -o \"/function/output\"",
                        WorkingDirectory = projectDir,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                _logger.LogInformation("Build output: {Output}", output);
                
                if (process.ExitCode != 0)
                {
                    _logger.LogError("Build failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                    throw new Exception($"Compilation failed: {error}");
                }
                
                _logger.LogInformation("Compilation successful");
                
                // Copy all published files from /function/output to function root
                var projectName = Path.GetFileNameWithoutExtension(projectFile);
                var publishOutputDir = "/function/output";
                var sourceDllPath = Path.Combine(publishOutputDir, $"{projectName}.dll");
                var destDllPath = Path.Combine("/function", $"{projectName}.dll");
                
                _logger.LogInformation("Looking for published DLL at: {SourcePath}", sourceDllPath);
                _logger.LogInformation("Will copy to: {DestPath}", destDllPath);
                
                if (Directory.Exists(publishOutputDir))
                {
                    var publishedFiles = Directory.GetFiles(publishOutputDir, "*.*");
                    _logger.LogInformation("Published files: {Files}", string.Join(", ", publishedFiles.Select(Path.GetFileName)));
                    
                    // Copy all published files to function root
                    foreach (var publishedFile in publishedFiles)
                    {
                        var fileName = Path.GetFileName(publishedFile);
                        var destPath = Path.Combine("/function", fileName);
                        if (System.IO.File.Exists(destPath))
                        {
                            System.IO.File.Delete(destPath); // Remove old version
                        }
                        System.IO.File.Copy(publishedFile, destPath);
                        _logger.LogInformation("Copied published file {FileName} to function directory", fileName);
                    }
                    
                    // Verify the main DLL was published and copied
                    if (System.IO.File.Exists(destDllPath))
                    {
                        var fileInfo = new FileInfo(destDllPath);
                        _logger.LogInformation("Verified DLL exists at {DestPath}, size: {Size} bytes", destDllPath, fileInfo.Length);
                    }
                    else
                    {
                        _logger.LogError("Main DLL not found after publish: {DestPath}", destDllPath);
                        throw new Exception($"Main DLL not found after publish: {destDllPath}");
                    }
                }
                else
                {
                    _logger.LogError("Publish output directory does not exist: {PublishDir}", publishOutputDir);
                    throw new Exception($"Publish output directory does not exist: {publishOutputDir}");
                }
                
                // Check what files exist after compilation
                var dllFiles = Directory.GetFiles("/function", "*.dll");
                _logger.LogInformation("DLL files after compilation: {Files}", string.Join(", ", dllFiles));
            }
            else
            {
                _logger.LogWarning("No .csproj files found for compilation");
                throw new Exception("No .csproj files found for compilation");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during compilation");
            throw;
        }
    }

    private void CopyRequiredLibraries()
    {
        try
        {
            // Copy Fission.DotNet.Common.dll to function directory for compilation
            var sourcePath = "/app/Fission.DotNet.Common.dll";
            var destPath = "/function/Fission.DotNet.Common.dll";
            
            if (System.IO.File.Exists(sourcePath))
            {
                _logger.LogInformation("Source path exists: {SourcePath}", sourcePath);
                if (!System.IO.File.Exists(destPath))
                {
                    System.IO.File.Copy(sourcePath, destPath);
                    _logger.LogInformation("Copied Fission.DotNet.Common.dll to function directory");
                    
                    // Verify the copy worked
                    if (System.IO.File.Exists(destPath))
                    {
                        _logger.LogInformation("Copy verified: {DestPath} exists", destPath);
                    }
                    else
                    {
                        _logger.LogError("Copy failed: {DestPath} does not exist after copy", destPath);
                    }
                }
                else
                {
                    _logger.LogInformation("Destination already exists: {DestPath}", destPath);
                }
            }
            else
            {
                _logger.LogError("Source path does not exist: {SourcePath}", sourcePath);
            }
            
            // Also copy other dependencies if needed
            var dependencies = new[]
            {
                "Microsoft.Extensions.DependencyInjection.Abstractions.dll"
            };
            
            foreach (var dep in dependencies)
            {
                var depSourcePath = $"/app/{dep}";
                var depDestPath = $"/function/{dep}";
                
                if (System.IO.File.Exists(depSourcePath) && !System.IO.File.Exists(depDestPath))
                {
                    System.IO.File.Copy(depSourcePath, depDestPath);
                    _logger.LogInformation("Copied {Dependency} to function directory", dep);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error copying required libraries");
        }
    }
}
