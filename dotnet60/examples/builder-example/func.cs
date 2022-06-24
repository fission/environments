using System;
using Fission.Functions;

public class HelloWorld : IFissionFunction
{
    public object Execute(FissionContext context)
    {
        string response = "initial value";
        try
        {
            context.Logger.WriteInfo("Starting...");
            response = MyClass.Instance.myValue.ToString();
        }  
        catch(Exception ex)
        {
            context.Logger.WriteError(ex.Message);
            response = ex.Message;
        }
        context.Logger.WriteInfo("Done");
        return response;
    }
}