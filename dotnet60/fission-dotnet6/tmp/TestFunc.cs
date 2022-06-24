using System;
using Fission.Functions;

/// <summary>
/// Test function for use when debugging.
/// </summary>
public class TestFunc : IFissionFunction
{
    /// <summary>
    /// Test function for use when debugging.
    /// </summary>
    /// <param name="context">Function call context.</param>
    /// <returns>Test parameters added together.</returns>
    public object Execute(FissionContext context)
    {
        context.Logger.WriteInfo ("Test message.");
        //var x = Convert.ToInt32(context.Arguments["x"]);
        //var y = Convert.ToInt32(context.Arguments["y"]); 
        //return (x + y);
        return "123";
    }
}
