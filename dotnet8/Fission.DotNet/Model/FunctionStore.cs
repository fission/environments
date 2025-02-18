using System;

namespace Fission.DotNet.Model;

public class FunctionStore
{
    public FunctionStore(string function)
    {
        if (string.IsNullOrEmpty(function))
            {
                throw new ArgumentException("Function string cannot be null or empty", nameof(function));
            }

            var parts = function.Split(':');
            if (parts.Length != 3)
            {
                throw new ArgumentException("Function string must contain exactly three parts separated by dots", nameof(function));
            }

            Assembly = $"{parts[0]}.dll";
            Namespace = parts[1];
            FunctionName = parts[2];
    }
    public string Assembly { get; private set; }
    public string Namespace { get; private set; }
    public string FunctionName { get; private set; }    
}
