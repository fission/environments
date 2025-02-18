#region header

// fission-dotnet6 - V2SpecializeController.cs

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

using Fission.DotNet.Properties;
using Fission.Common;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

#endregion

namespace Fission.DotNet.Controllers
{
    /// <summary>
    ///     Controller to handle specializing the container to handle a particular Fission function.
    /// </summary>
    /// <remarks>
    ///     Essentially, this handles fetching, compiling, and caching the function for later use by the
    ///     <see cref="FunctionController" />.
    /// </remarks>
    [Route (template: "/v2/specialize")]
    [ApiController]
    public class V2SpecializeController : ControllerBase
    {
        private readonly ILogger<V2SpecializeController> logger;
        private readonly IFunctionStore                  store;

        public string GetBodyAsString ()
        {
            var length = (int) Request.ContentLength;
            var data   = new byte[length];
            Request.Body.Read(buffer: data, offset: 0, count: length);

            return System.Text.Encoding.UTF8.GetString(bytes: data);
        }
        /// <summary>
        ///     Creates an instance of the <see cref="V2SpecializeController" />.
        /// </summary>
        /// <param name="logger">A logger instance for the <see cref="V2SpecializeController" />.</param>
        /// <param name="store">The function store service. See <see cref="IFunctionStore" />.</param>
        public V2SpecializeController (ILogger<V2SpecializeController> logger, IFunctionStore store)
        {
            this.logger = logger;
            this.store  = store;
        }

        /// <summary>
        ///     Handle version 2 requests to specialize the container; i.e., to compile and cache a multi-file function.
        /// </summary>
        /// <returns>200 OK on success; 500 Internal Server Error on failure.</returns>
        [HttpPost]
        [NotNull]
        public object Post ()
        {
            this.logger.LogInformation (message: "/v2/specialize called.");
            var errors = new List<string>();
            var oinfo = new List<string>();
            var body = GetBodyAsString();
            Console.WriteLine($"Request received by endpoint from builder: {body}");
            var builderRequest = System.Text.Json.JsonSerializer.Deserialize<BuilderRequest>(body);

            string functionPath = string.Empty;

            store.SetPackagePath(builderRequest.filepath);

            // following will enable us to skip --entrypoint flag during function creation 
            if (!string.IsNullOrWhiteSpace(builderRequest.functionName))
            {
                functionPath = System.IO.Path.Combine(builderRequest.filepath, $"{builderRequest.functionName}.cs");
            }
            else
            {
                functionPath = System.IO.Path.Combine(builderRequest.filepath, CompilerHelper.Instance.builderSettings.functionBodyFileName);
            }

            Console.WriteLine($"Going to read function body from path: {functionPath}");

            if (!System.IO.File.Exists(functionPath))
            {
                var error = $"Unable to locate function source code at '{functionPath}'.";
                this.logger.LogError(message: error);
                return this.StatusCode(statusCode:(int) HttpStatusCode.InternalServerError, value: error);
            }
            var code = System.IO.File.ReadAllText(functionPath);
            FunctionRef? binary = null;
            try
            {
                FissionCompiler fissionCompiler = new FissionCompiler();
                binary = fissionCompiler.CompileV2(builderRequest.filepath, out errors, out oinfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting function: {ex.Message}, Trace: {ex.StackTrace}");
            }
            if (binary == null)
            {
                string? error = string.Join(separator: Environment.NewLine, values: errors);
                this.logger.LogError(message: error);
                return this.StatusCode(statusCode:(int) HttpStatusCode.InternalServerError, value: error);
            }
            else
            {
                store.SetFunctionRef(binary);
            }
            return this.Ok();
        }
    }
}
