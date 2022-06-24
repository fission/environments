using System;
using Fission.Functions;

public class HelloWorld : IFissionFunction
{
    public object Execute(FissionContext context)
    {
        return "hello, world!";
    }
}
