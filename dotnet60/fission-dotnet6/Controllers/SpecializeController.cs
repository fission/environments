#region header

// fission-dotnet6 - SpecializeController.cs
// 
// Created by: Alistair J R Young(avatar) at 2020/12/29 12:10 AM.
// Modified by: Vsevolod Kvachev (Rasie1) at 2022.

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Net;

using Fission.DotNet.Properties;

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
    [Route(template: "/specialize")]
    [ApiController]
    public class SpecializeController : ControllerBase
    {
        private readonly ILogger<SpecializeController> logger;
        private readonly IFunctionStore                store;

        /// <summary>
        ///     Creates an instance of the <see cref="SpecializeController" />.
        /// </summary>
        /// <param name="logger">A logger instance for the <see cref="SpecializeController" />.</param>
        /// <param name="store">The function store service. See <see cref="IFunctionStore" />.</param>
        public SpecializeController(ILogger<SpecializeController> logger, IFunctionStore store)
        {
            this.logger = logger;
            this.store  = store;
        }

        /// <summary>
        ///     The path to the function code to compile. In Debug builds, this invokes a built-in test function to simplify
        ///     debugging without a container. In Release builds, this uses the mount path of the function package.
        /// </summary>
        [NotNull]
        private static string CodePath
#if DEBUG
            => "tmp/TestFunc.cs";
#else
            => "/userfunc/user";
#endif

        /// <summary>
        ///     Handle version 1 requests to specialize the container; i.e., to compile and cache a single-file function.
        /// </summary>
        /// <returns>200 OK on success; 500 Internal Server Error on failure.</returns>
        [HttpPost]
        [NotNull]
        public object Post()
        {
            this.logger.LogInformation(message: "/specialize called.");

            if (System.IO.File.Exists(path: SpecializeController.CodePath))
            {
                string source = System.IO.File.ReadAllText(path: SpecializeController.CodePath);

                var          compiler = new FissionCompiler();
                FunctionRef? binary   = compiler.Compile(source: source, errors: out List<string> errors);

                if (binary == null)
                {
                    string? error = string.Join(separator: Environment.NewLine, values: errors);
                    this.logger.LogError(message: error);
                    return this.StatusCode(statusCode:(int) HttpStatusCode.InternalServerError, value: error);
                }

                this.store.SetFunctionRef(func: binary);
            }
            else
            {
                var error = $"Unable to locate function source code at '{SpecializeController.CodePath}'.";
                this.logger.LogError(message: error);
                return this.StatusCode(statusCode:(int) HttpStatusCode.InternalServerError, value: error);
            }

            return this.Ok();
        }
    }
}
