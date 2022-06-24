#region header

// fission-dotnet6 - FunctionStore.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/29 10:41 AM.
// Modified by: Vsevolod Kvachev (Rasie1) at 2022.

#endregion

#region using

using System;

using Fission.DotNet.Properties;

#endregion

namespace Fission.DotNet
{
    /// <summary>
    ///     Implementation of the function store service, as defined by <see cref="IFunctionStore" />.
    /// </summary>
    internal class FunctionStore : IFunctionStore
    {
        private FunctionRef? func;
        private string? packagePath;

        /// <inheritdoc />
        FunctionRef? IFunctionStore.Func => this.func;

        /// <inheritdoc />
        void IFunctionStore.SetFunctionRef(FunctionRef func)
        {
            this.func = func;
        }
        
        /// <inheritdoc />
        string? IFunctionStore.PackagePath => this.packagePath;

        /// <inheritdoc />
        void IFunctionStore.SetPackagePath(string packagePath)
        {
            this.packagePath = packagePath;
        }
    }
}
