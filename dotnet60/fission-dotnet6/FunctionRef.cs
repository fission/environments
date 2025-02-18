#region header

// fission-dotnet6 - FunctionRef.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/29 9:45 AM.
// Modified by: Vsevolod Kvachev (Rasie1) at 2022.

#endregion

#region using

using System;
using System.Reflection;

using Fission.Functions;

#endregion

namespace Fission.DotNet
{
    /// <summary>
    ///     A reference to a Fission function, used to invoke it.
    /// </summary>
    public class FunctionRef
    {
        private readonly Assembly assembly;
        private readonly Type     type;

        /// <summary>
        ///     Create an instance of a Fission function.
        /// </summary>
        /// <param name="assembly">The assembly containing the Fission function.</param>
        /// <param name="type">The type, implementing <see cref="IFissionFunction" />, containing the Fission function.</param>
        public FunctionRef (Assembly assembly, Type type)
        {
            this.assembly = assembly;
            this.type     = type;
        }

        /// <summary>
        ///     Invoke the Fission function referenced by this FunctionRef instance.
        /// </summary>
        /// <param name="context">The function invocation context.</param>
        /// <returns>The Fission function return value.</returns>
        public object Invoke (FissionContext context)
            => ((IFissionFunction) this.assembly.CreateInstance (typeName: this.type.FullName!)!)!.Execute (context: context);
    }
}
