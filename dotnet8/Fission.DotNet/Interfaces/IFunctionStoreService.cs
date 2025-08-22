using System;
using Fission.DotNet.Model;

namespace Fission.DotNet.Interfaces;

public interface IFunctionStoreService
{
    void SetFunction(FunctionStore function);
    FunctionStore GetFunction();
}
