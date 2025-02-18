using System;

namespace Fission.DotNet.Common
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogDebug(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogCritical(string message);
        void LogError(string message, Exception exception);
    }
}