using Fission.DotNet.Common;

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
