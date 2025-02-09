using Fission.DotNet.Common;
using System.Threading.Tasks;

public class MyFunction
{
    public async Task<object> ExecuteAsync(FissionContext context)
    {
        await Task.Delay(1000); // Simulate an asynchronous operation
        return "Hello from async function!";
    }
}
