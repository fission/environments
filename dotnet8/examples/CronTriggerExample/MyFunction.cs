using Fission.DotNet.Common;

public class MyFunction
{
    public object Execute(FissionContext context)
    {
        return "Hello from Cron trigger!";
    }
}
