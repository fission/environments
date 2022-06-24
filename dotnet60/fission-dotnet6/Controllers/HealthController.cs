#region header

// fission-dotnet6 - HealthController.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/29 12:10 AM.

#endregion

#region using

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc;

#endregion

namespace Fission.DotNet.Controllers
{
    /// <summary>
    ///     Controller to handle the Docker/Kubernetes container health-check.
    /// </summary>
    [Route (template: "/healthz")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        ///     When this endpoint receives a GET request, simply return 200 OK to demonstrate that the container is alive.
        /// </summary>
        /// <returns>200 OK</returns>
        [HttpGet]
        [NotNull]
        public object Get () => this.Ok ();
    }
}
