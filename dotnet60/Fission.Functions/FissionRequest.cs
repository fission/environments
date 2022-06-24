#region header

// Fission.Functions - FissionRequest.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/30 8:08 AM.

#endregion

#region using

using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using JetBrains.Annotations;

#endregion

namespace Fission.Functions
{
    /// <summary>
    ///     Underlying request details supplied to the handling function.
    /// </summary>
    [PublicAPI]
    public record FissionRequest
    {
        /// <summary>
        ///     The body of the request, supplied as a readable <see cref="Stream" />. See also
        ///     <seealso cref="FissionRequest.GetBodyAsString" /> to receive the entire body of the request as a single
        ///     <see cref="string" />.
        /// </summary>
        public Stream Body;

        /// <summary>
        ///     The client certificate supplied with the request.
        /// </summary>
        [CanBeNull]
        public X509Certificate2 ClientCertificate;

        /// <summary>
        ///     The HTTP headers supplied with the request.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers;

        /// <summary>
        ///     The HTTP method used to make the request (i.e., "GET", "POST", etc.).
        /// </summary>
        public string Method;

        /// <summary>
        ///     The URL used to make the request, formatted as a string.
        /// </summary>
        /// <remarks>
        ///     The URL is urlencoded, and includes all elements, including (for example) the query string.
        /// </remarks>
        public string Url;

        /// <summary>
        ///     Get the body of the request as a single <see cref="string" />.
        /// </summary>
        /// <returns>The body of the request as a single <see cref="string" />.</returns>
        [NotNull]
        public string GetBodyAsString ()
        {
            var length = (int) this.Body.Length;
            var data   = new byte[length];
            this.Body.Read (buffer: data, offset: 0, count: length);

            return Encoding.UTF8.GetString (bytes: data);
        }
    }
}
