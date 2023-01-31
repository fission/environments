#region header

// Fission.Functions - IFissionFunction.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/28 11:29 PM.

#endregion

#region using

using JetBrains.Annotations;

#endregion

namespace Fission.Functions
{
    /// <summary>
    ///     The interface which is implemented by Fission functions, permitting the environment container to identify possible
    ///     entry
    ///     points.
    /// </summary>
    [PublicAPI]
    public interface IFissionFunction
    {
        /// <summary>
        ///     The entry point of a Fission function.
        /// </summary>
        /// <param name="context">The function call context supplied by the environment container.</param>
        /// <returns>
        ///     An object which will be appropriately formatted by the environment container and returned to the
        ///     caller.
        /// </returns>
        public object Execute (FissionContext context);
    }
}
