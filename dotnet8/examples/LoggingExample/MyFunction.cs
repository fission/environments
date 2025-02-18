using Fission.DotNet.Common;
using System.Threading.Tasks;

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
