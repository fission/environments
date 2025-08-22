using System;
using Fission.DotNet.Model;

namespace Fission.DotNet.Interfaces;

public interface ISpecializeService
{
    void Specialize(FissionSpecializeRequest request);
}
