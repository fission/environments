using System;
using Fission.DotNet.Common;
using Fission.DotNet.Interfaces;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Fission.DotNet.Adapter;

namespace Fission.DotNet.Services;

public class FunctionService : IFunctionService
{
    private readonly IFunctionStoreService _functionStoreService;
    private readonly ILogger<FunctionService> _logger;
    private CustomAssemblyLoadContext _alc;
    private WeakReference _alcWeakRef;
    private Assembly _assemblyFunction;
    private Type _classFunctionNameType;

    public FunctionService(IFunctionStoreService functionStoreService, ILogger<FunctionService> logger)
    {
        this._functionStoreService = functionStoreService;
        this._logger = logger;
    }

    public ICorsPolicy GetCorsPolicy()
    {
        _logger.LogInformation("GetCorsPolicy");
        var policy = new CorsPolicy();                

        if (_classFunctionNameType != null)
        {
            // Method to execute
            var executeMethod = _classFunctionNameType.GetMethod("SetCorsPolicy", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (executeMethod != null)
            {
                executeMethod.Invoke(null, new object[] { policy });
                var headers = policy.GetCorsHeaders();
                _logger.LogInformation($"CorsPolicy: {string.Join(", ", headers)}");
            }
        }
        else
        {
            throw new Exception("Type not found.");
        }

        return policy;
    }

    public Task<object> Execute(FissionContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        /*var function = this._functionStoreService.GetFunction();

        if (function == null)
        {
            throw new Exception("Function not specialized.");
        }

        return ExecuteAndUnload(function.Assembly, function.Namespace, function.FunctionName, context);*/
        return ExecuteMethod(context);
    }

    /*[MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<object> ExecuteAndUnload(string assemblyPath, string nameSpace, string classFunctionName, FissionContext context)
    {
        ExecuteMethod(context);
        /*if (!System.IO.File.Exists($"/function/{assemblyPath}"))
        {
            throw new Exception($"File /function/{assemblyPath} not found.");
        }

        // Delete the common library if it exists. This is to ensure that the latest version is always loaded.
        if (File.Exists($"/function/Fission.DotNet.Common.dll"))
        {
            File.Delete($"/function/Fission.DotNet.Common.dll");
        }

        if (File.Exists($"/function/Microsoft.Extensions.DependencyInjection.Abstractions.dll"))
        {
            File.Delete($"/function/Microsoft.Extensions.DependencyInjection.Abstractions.dll");
        }

        WeakReference alcWeakRef = null;

        var alc = new CustomAssemblyLoadContext($"/function/{assemblyPath}", isCollectible: true);
        try
        {
            var assemblyFunction = alc.LoadFromAssemblyPath($"/function/{assemblyPath}");

            alcWeakRef = new WeakReference(alc, trackResurrection: true);

            // Ottieni tutte le classi nell'assembly
            Type[] types = assemblyFunction.GetTypes();
            _logger.LogInformation("Elenco delle classi nell'assembly:");
            foreach (var type1 in types)
            {
                _logger.LogInformation(type1.FullName);
            }

            _logger.LogInformation($"Class try found: {nameSpace}.{classFunctionName}");

            var classFunctionNameType = assemblyFunction.GetType($"{nameSpace}.{classFunctionName}");

            if (classFunctionNameType != null)
            {
                MethodInfo configureServicesMethod = classFunctionNameType.GetMethod("ConfigureServices", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                ServiceCollection serviceCollection = null;
                ServiceProvider serviceProvider = null;
                if (configureServicesMethod != null)
                {
                    serviceCollection = new ServiceCollection();
                    configureServicesMethod.Invoke(null, new object[] { serviceCollection });
                    serviceCollection.AddTransient(classFunctionNameType);
                    serviceProvider = serviceCollection.BuildServiceProvider();
                }

                // Method to execute
                var executeMethod = classFunctionNameType.GetMethod("Execute");

                if (executeMethod != null)
                {
                    // Create an instance of the object
                    object classInstance = null;

                    if (serviceProvider != null)
                    {
                        classInstance = serviceProvider.GetService(classFunctionNameType);
                    }
                    else
                    {
                        classInstance = Activator.CreateInstance(classFunctionNameType);
                    }

                    if (classInstance == null)
                    {
                        throw new Exception("Instance not created.");
                    }

                    var executeMethodParameters = new object[] { context };

                    _logger.LogDebug($"Executing {classFunctionNameType.FullName}.{executeMethod.Name}");

                    var parameters = executeMethod.GetParameters();
                    if (parameters.Length > 1)
                    {
                        _logger.LogDebug($"Method {executeMethod.Name} has more than one parameter.");
                        if (parameters[1].ParameterType == typeof(Common.ILogger))
                        {
                            executeMethodParameters = new object[] { context, new FissionLoggerAdapter(_logger) };
                        }
                    }

                    _logger.LogDebug($"Method {executeMethod.Name} has {parameters.Length} parameters.");

                    // Execute the method
                    var result = executeMethod.Invoke(classInstance, executeMethodParameters);

                    if (result is Task task)
                    {
                        _logger.LogInformation("Task found.");
                        await task.ConfigureAwait(false);
                        var taskType = task.GetType();
                        if (taskType.IsGenericType)
                        {
                            return taskType.GetProperty("Result").GetValue(task);
                        }
                        return null;
                    }
                    else
                    {
                        _logger.LogInformation("Task not found.");
                    }

                    return result;
                }
                else
                {
                    throw new Exception("Method not found.");
                }
            }
            else
            {
                throw new Exception("Type not found.");
            }
        }
        finally
        {
            alc.Unload();

            for (int i = 0; alcWeakRef.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }*
    }*/

    public void Load()
    {
        var function = this._functionStoreService.GetFunction();

        if (function == null)
        {
            throw new Exception("Function not specialized.");
        }

        LoadAssembly(function.Assembly, function.Namespace, function.FunctionName);
    }

    public void Unload()
    {
        if (_alc != null)
        {
            _alc.Unload();

            for (int i = 0; _alcWeakRef != null && _alcWeakRef.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }



    private void LoadAssembly(string assemblyPath, string nameSpace, string classFunctionName)
    {
        if (!System.IO.File.Exists($"/function/{assemblyPath}"))
        {
            throw new Exception($"File /function/{assemblyPath} not found.");
        }

        // Delete the common library if it exists. This is to ensure that the latest version is always loaded.
        if (File.Exists($"/function/Fission.DotNet.Common.dll"))
        {
            File.Delete($"/function/Fission.DotNet.Common.dll");
        }

        if (File.Exists($"/function/Microsoft.Extensions.DependencyInjection.Abstractions.dll"))
        {
            File.Delete($"/function/Microsoft.Extensions.DependencyInjection.Abstractions.dll");
        }

        // Try simpler assembly loading first for debugging
        try
        {
            _logger.LogInformation("Attempting standard Assembly.LoadFrom for debugging");
            _assemblyFunction = Assembly.LoadFrom($"/function/{assemblyPath}");
            _logger.LogInformation("Standard Assembly.LoadFrom succeeded");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Standard Assembly.LoadFrom failed, trying CustomAssemblyLoadContext");
            
            _alc = new CustomAssemblyLoadContext($"/function/{assemblyPath}", isCollectible: true);
            
            // Ensure dependencies are available in the load context
            var fissionCommonPath = "/function/Fission.DotNet.Common.dll";
            if (System.IO.File.Exists(fissionCommonPath))
            {
                _logger.LogInformation("Loading Fission.DotNet.Common.dll dependency");
                _alc.LoadFromAssemblyPath(fissionCommonPath);
            }
            
            _assemblyFunction = _alc.LoadFromAssemblyPath($"/function/{assemblyPath}");
            _alcWeakRef = new WeakReference(_alc, trackResurrection: true);
        }

        // First try with namespace
        _classFunctionNameType = _assemblyFunction.GetType($"{nameSpace}.{classFunctionName}");
        
        // If not found with namespace, try without namespace (global namespace)
        if (_classFunctionNameType == null)
        {
            _logger.LogInformation("Type {TypeName} not found with namespace, trying without namespace", $"{nameSpace}.{classFunctionName}");
            _classFunctionNameType = _assemblyFunction.GetType(classFunctionName);
        }
        
        // If still not found, list all available types for debugging
        if (_classFunctionNameType == null)
        {
            try 
            {
                _logger.LogInformation("Assembly full name: {AssemblyFullName}", _assemblyFunction.FullName);
                _logger.LogInformation("Assembly location: {Location}", _assemblyFunction.Location);
                _logger.LogInformation("Assembly entry point: {EntryPoint}", _assemblyFunction.EntryPoint?.Name ?? "None");
                
                var allTypes = _assemblyFunction.GetTypes();
                _logger.LogInformation("Total types in assembly: {TypeCount}", allTypes.Length);
                
                var availableTypes = allTypes.Select(t => t.FullName ?? t.Name).ToArray();
                _logger.LogError("Type {TypeName} not found. Available types: {AvailableTypes}", classFunctionName, string.Join(", ", availableTypes));
                
                // Additional debugging - check if assembly has any exported types
                var exportedTypes = _assemblyFunction.GetExportedTypes().Select(t => t.FullName ?? t.Name).ToArray();
                _logger.LogInformation("Exported types: {ExportedTypes}", string.Join(", ", exportedTypes));
                
                // Check assembly file path and if it exists
                _logger.LogInformation("Assembly loaded from: {Location}, Exists: {FileExists}", 
                    _assemblyFunction.Location, System.IO.File.Exists(_assemblyFunction.Location));
                    
                // Check if the assembly file has any content
                if (System.IO.File.Exists(_assemblyFunction.Location))
                {
                    var fileInfo = new System.IO.FileInfo(_assemblyFunction.Location);
                    _logger.LogInformation("Assembly file size: {FileSize} bytes", fileInfo.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting types from assembly");
            }
            
            throw new Exception("Type not found.");
        }
        
        _logger.LogInformation("Successfully found type: {TypeName}", _classFunctionNameType.FullName);
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private async Task<object> ExecuteMethod(FissionContext context)
    {
        if (_classFunctionNameType != null)
        {
            MethodInfo configureServicesMethod = _classFunctionNameType.GetMethod("ConfigureServices", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            ServiceProvider serviceProvider = null;
            if (configureServicesMethod != null)
            {
                var serviceCollection = new ServiceCollection();
                configureServicesMethod.Invoke(null, new object[] { serviceCollection });
                serviceCollection.AddTransient(_classFunctionNameType);
                serviceProvider = serviceCollection.BuildServiceProvider();
            }

            // Method to execute
            var executeMethod = _classFunctionNameType.GetMethod("Execute");

            if (executeMethod != null)
            {
                // Create an instance of the object
                object classInstance = null;

                if (serviceProvider != null)
                {
                    classInstance = serviceProvider.GetService(_classFunctionNameType);
                }
                else
                {
                    classInstance = Activator.CreateInstance(_classFunctionNameType);
                }

                if (classInstance == null)
                {
                    throw new Exception("Instance not created.");
                }

                var executeMethodParameters = new object[] { context };

                _logger.LogDebug($"Executing {_classFunctionNameType.FullName}.{executeMethod.Name}");

                var parameters = executeMethod.GetParameters();
                if (parameters.Length > 1)
                {
                    _logger.LogDebug($"Method {executeMethod.Name} has more than one parameter.");
                    if (parameters[1].ParameterType == typeof(Common.ILogger))
                    {
                        executeMethodParameters = new object[] { context, new FissionLoggerAdapter(_logger) };
                    }
                }

                _logger.LogDebug($"Method {executeMethod.Name} has {parameters.Length} parameters.");

                // Execute the method
                var result = executeMethod.Invoke(classInstance, executeMethodParameters);

                if (result is Task task)
                {
                    _logger.LogInformation("Task found.");
                    await task.ConfigureAwait(false);
                    var taskType = task.GetType();
                    if (taskType.IsGenericType)
                    {
                        return taskType.GetProperty("Result").GetValue(task);
                    }
                    return null;
                }
                else
                {
                    _logger.LogInformation("Task not found.");
                }

                return result;
            }
            else
            {
                throw new Exception("Method not found.");
            }
        }
        else
        {
            throw new Exception("Type not found.");
        }


        /*MethodInfo method = _classFunctionNameType.GetMethod(methodName);

        if (method == null)
        {
            throw new Exception("Method not found.");
        }

        object classInstance = Activator.CreateInstance(_classFunctionNameType);
        var executeMethodParameters = new object[] { context };

        var parameters = method.GetParameters();
        if (parameters.Length > 1 && parameters[1].ParameterType == typeof(Common.ILogger))
        {
            executeMethodParameters = new object[] { context, new FissionLoggerAdapter(_logger) };
        }

        var result = method.Invoke(classInstance, executeMethodParameters);

        if (result is Task task)
        {
            await task.ConfigureAwait(false);
            var taskType = task.GetType();
            if (taskType.IsGenericType)
            {
                return taskType.GetProperty("Result").GetValue(task);
            }
            return null;
        }

        return result;*/
    }
}

