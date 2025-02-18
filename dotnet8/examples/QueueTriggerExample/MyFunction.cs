using Fission.DotNet.Common;

public class MyFunction
{
    public object Execute(FissionContext context)
    {
        if (context is FissionMqContext request)
        {
            return $"Hello from Queue trigger! Topic: {request.Topic}";
        }
        return "Invalid context";
    }
}
