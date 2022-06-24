#region header

// fission-dotnet6 - FunctionController.cs
// 
// Created by: Alistair J R Young(avatar) at 2020/12/28 11:19 PM.

#endregion

#region using

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

using Fission.DotNet.Properties;
using Fission.Functions;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

#endregion

namespace Fission.DotNet.Controllers
{
    /// <summary>
    ///     Controller handling calls to the Fission function in this environment container.
    /// </summary>
    [ApiController]
    [Route(template: "/")]
    public class FunctionController : ControllerBase
    {
        private readonly ILogger<IFissionFunction>   funcLogger;
        private readonly ILogger<FunctionController> logger;
        private readonly IFunctionStore              store;

        /// <summary>
        ///     Creates an instance of this <see cref="FunctionController" />.
        /// </summary>
        /// <param name="logger">A logger for the <see cref="FunctionController" />.</param>
        /// <param name="funcLogger">A logger for the function invoked by the <see cref="FunctionController" />.</param>
        /// <param name="store">The function storage service(see <see cref="IFunctionStore" />).</param>
        /// <remarks>
        ///     The second logger exists to permit ready differentiation of function-internal errors and other messages from those
        ///     originating with the host environment.
        /// </remarks>
        public FunctionController(ILogger<FunctionController> logger, ILogger<IFissionFunction> funcLogger, IFunctionStore store)
        {
            this.logger     = logger;
            this.funcLogger = funcLogger;
            this.store      = store;
        }

        /// <summary>
        ///     Handle HTTP GET requests by forwarding to the Fission function.
        /// </summary>
        /// <returns>The function return value.</returns>
        [HttpGet]
        [NotNull]
        public object Get() => this.Run();

        /// <summary>
        ///     Handle HTTP POST requests by forwarding to the Fission function.
        /// </summary>
        /// <returns>The function return value.</returns>
        [HttpPost]
        [NotNull]
        public object Post() => this.Run();

        /// <summary>
        ///     Handle HTTP PUT requests by forwarding to the Fission function.
        /// </summary>
        /// <returns>The function return value.</returns>
        [HttpPut]
        [NotNull]
        public object Put() => this.Run();

        /// <summary>
        ///     Handle HTTP HEAD requests by forwarding to the Fission function.
        /// </summary>
        /// <returns>The function return value.</returns>
        [HttpHead]
        [NotNull]
        public object Head() => this.Run();

        /// <summary>
        ///     Handle HTTP OPTIONS requests by forwarding to the Fission function.
        /// </summary>
        /// <returns>The function return value.</returns>
        [HttpOptions]
        [NotNull]
        public object Options() => this.Run();

        /// <summary>
        ///     Handle HTTP DELETE requests by forwarding to the Fission function.
        /// </summary>
        /// <returns>The function return value.</returns>
        [HttpDelete]
        [NotNull]
        public object Delete() => this.Run();

        /// <summary>
        ///     Invokes the Fission function on behalf of the caller.
        /// </summary>
        /// <returns>
        ///     200 OK with the Fission function return value; or 400 Bad Request with the exception message if an exception
        ///     occurred in the Fission function; or 500 Internal Server Error if the environment container has not yet been
        ///     specialized.
        /// </returns>
        [NotNull]
        private object Run()
        {
            this.logger.LogInformation(message: "Invoking function");

            if(this.store.Func == null)
            {
                this.logger.LogError(message: Resources.FunctionController_Run_GenericContainer);

                return this.StatusCode(statusCode:(int) HttpStatusCode.InternalServerError,
                                        value: Resources.FunctionController_Run_GenericContainer);
            }

            try
            {
                FissionContext context = this.BuildContext();
                this.logger.LogInformation(message: "Context built");
                return this.Ok(value: this.store.Func.Invoke(context: context));
            }
            catch (Exception e)
            {
                this.logger.LogError(message: e.Message);
                return this.StatusCode(statusCode:(int) HttpStatusCode.BadRequest, value: e.Message);
            }
        }

        /// <summary>
        ///     Build the context for the Fission function, assembling data from the call request and the function logger.
        /// </summary>
        /// <param name="packagePath">The path to the function package.</param>
        /// <returns>A <see cref="FissionContext" /> to be passed to the Fission function.</returns>
        private FissionContext BuildContext(string packagePath = "")
        {
            var fl = new FissionLogger
                     {
                         WriteInfo     =(format, objects) => this.funcLogger.LogInformation(message: format, args: objects),
                         WriteWarning  =(format, objects) => this.funcLogger.LogWarning(message: format, args: objects),
                         WriteError    =(format, objects) => this.funcLogger.LogError(message: format, args: objects),
                         WriteCritical =(format, objects) => this.funcLogger.LogCritical(message: format, args: objects),
                     };

            IQueryCollection arguments = this.Request.Query;

            var args = new Dictionary<string, string>();

            foreach(var k in arguments.Keys) args[key: k] = arguments[key: k];

            var headers = new Dictionary<string, IEnumerable<string>>();

            foreach(KeyValuePair<string, StringValues> kv in this.Request.Headers)
                headers.Add(key: kv.Key, value: kv.Value);

            var fr = new FissionRequest
                     {
                         Body              = this.Request.Body,
                         ClientCertificate = this.Request.HttpContext.Connection.ClientCertificate,
                         Headers           = new ReadOnlyDictionary<string, IEnumerable<string>>(dictionary: headers),
                         Method            = this.Request.Method,
                         Url               = this.Request.GetEncodedUrl(),
                     };

            var fc = new FissionContext
                     {
                         Logger      = fl,
                         Arguments   = new ReadOnlyDictionary<string, string>(dictionary: args),
                         Request     = fr,
                         PackagePath = packagePath,
                     };

            return fc;
        }
    }
}
