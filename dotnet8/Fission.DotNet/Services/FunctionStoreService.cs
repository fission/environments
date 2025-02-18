using System;
using Fission.DotNet.Interfaces;
using Fission.DotNet.Model;

namespace Fission.DotNet.Services;

public class FunctionStoreService : IFunctionStoreService
{
    private FunctionStore _function;
    public FunctionStore GetFunction()
    {
        return _function;
    }

    public void SetFunction(FunctionStore function)
    {
        _function = function;
    }
}
