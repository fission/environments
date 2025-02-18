#region header

// Fission.Functions - FissionLogger.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/30 8:08 AM.

#endregion

#region using

using JetBrains.Annotations;

#endregion

namespace Fission.Functions
{
    /// <summary>
    ///     A delegate defining a function capable of performing Fission function logging; i.e., a possible FissionLogger
    ///     function.
    /// </summary>
    /// <param name="format">The message to log, in the form of a format string for the following arguments, if any.</param>
    /// <param name="args">Any arguments to log (parameters for the format string).</param>
    public delegate void FissionWriteLog (string format, params object[] args);

    /// <summary>
    ///     Logging functions for the receiving Fission function.
    /// </summary>
    [PublicAPI]
    public record FissionLogger
    {
        /// <summary>
        ///     Log a message reporting a critical error.
        /// </summary>
        public FissionWriteLog WriteCritical;

        /// <summary>
        ///     Log a message reporting an error.
        /// </summary>
        public FissionWriteLog WriteError;

        /// <summary>
        ///     Log an informational message.
        /// </summary>
        public FissionWriteLog WriteInfo;

        /// <summary>
        ///     Log a warning message.
        /// </summary>
        public FissionWriteLog WriteWarning;
    }
}
