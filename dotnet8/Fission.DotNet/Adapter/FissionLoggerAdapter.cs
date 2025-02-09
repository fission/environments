using System;
using Fission.DotNet.Common;

namespace Fission.DotNet.Adapter;

public class FissionLoggerAdapter : Common.ILogger
{
    private Microsoft.Extensions.Logging.ILogger _logger;

    public FissionLoggerAdapter(Microsoft.Extensions.Logging.ILogger logger)
    {
        this._logger = logger;
    }

    public void LogCritical(string message)
    {
        _logger.LogCritical(message);
    }

    public void LogDebug(string message)
    {
        _logger.LogDebug(message);
    }

    public void LogError(string message, Exception exception)
    {
        _logger.LogError(exception, message);
    }

    public void LogError(string message)
    {
        _logger.LogError(message);
    }

    public void LogInformation(string message)
    {
        _logger.LogInformation(message);
    }

    public void LogWarning(string message)
    {
        _logger.LogWarning(message);
    }
}
