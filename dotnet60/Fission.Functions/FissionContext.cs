#region header

// Fission.Functions - FissionContext.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/28 11:30 PM.

#endregion

#region using

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

using JetBrains.Annotations;

#endregion

namespace Fission.Functions
{
    /// <summary>
    ///     The context supplied to Fission functions by the .NET 5 environment, including arguments, HTTP request information,
    ///     logging facilities, and (for built functions) settings.
    /// </summary>
    [PublicAPI]
    public record FissionContext
    {
        /// <summary>
        ///     The arguments specified to the Fission function, derived from the HTTP request's query string.
        /// </summary>
        public IReadOnlyDictionary<string, string> Arguments;

        /// <summary>
        ///     Functions permitting the Fission function to write to the container log.
        /// </summary>
        public FissionLogger Logger;

        /// <summary>
        ///     The path to the location in which the Fission function package is loaded into the container.
        /// </summary>
        /// <remarks>
        ///     This is primarily used internally to fetch the location for JSON settings files.
        /// </remarks>
        public string PackagePath;

        /// <summary>
        ///     Details of the HTTP request which generated the function call.
        /// </summary>
        public FissionRequest Request;

        /// <summary>
        ///     Read a JSON settings file supplied in the function package, and deserialize it into a corresponding .NET object.
        /// </summary>
        /// <typeparam name="T">Type of the .NET object corresponding to the settings file.</typeparam>
        /// <param name="relativePath">Path, beneath <see cref="PackagePath" />, of the JSON settings file.</param>
        /// <returns>A .NET object corresponding to the JSON settings file.</returns>
        [CanBeNull]
        public T GetSettings<T> ([NotNull] string relativePath)
        {
            string filePath = Path.Combine (path1: this.PackagePath, path2: relativePath);
            string json     = File.ReadAllText (path: filePath);

            return JsonSerializer.Deserialize<T> (json: json);
        }
    }
}
