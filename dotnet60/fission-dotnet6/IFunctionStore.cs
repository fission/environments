#region header

// fission-dotnet6 - IFunctionStore.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/29 10:39 AM.

#endregion

namespace Fission.DotNet
{
    /// <summary>
    ///     Interface for the function store service, which caches the post-specialization function for repeated use by the
    ///     <see cref="Controllers.FunctionController" />.
    /// </summary>
    public interface IFunctionStore
    {
        public FunctionRef? Func { get; }

        public void SetFunctionRef(FunctionRef func);

        public string? PackagePath { get; }

        public void SetPackagePath(string func);
    }
}
