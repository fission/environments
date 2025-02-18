using System;
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

        if (string.IsNullOrEmpty(request.functionName))
        {
            throw new ArgumentNullException("Function name is required");
        }

        var functionStore = new FunctionStore(request.functionName);
        _functionStoreService.SetFunction(functionStore);
    }

    private void UnzipDeployArchive(string zipFilePath, string extractPath)
    {
        if (System.IO.File.Exists(zipFilePath))
        {
            // Create the /function folder if it does not exist
            if (!Directory.Exists(extractPath))
            {
                Directory.CreateDirectory(extractPath);
            }

            // Unzip the ZIP file into the /function folder
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            _logger.LogInformation("Deploy archive unzipped to {ExtractPath}", extractPath);
        }
        else
        {
            _logger.LogWarning("Deploy archive does not exist at {ZipFilePath}", zipFilePath);
        }
    }
}
