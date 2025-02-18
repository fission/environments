using System;
using Fission.DotNet.Common;

namespace Fission.DotNet.Interfaces;

public interface IFunctionService
{
    void Load();
    void Unload();
    Task<object> Execute(FissionContext context);
    ICorsPolicy GetCorsPolicy();
}
