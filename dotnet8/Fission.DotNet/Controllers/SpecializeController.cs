using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Fission.DotNet.Interfaces;
using Fission.DotNet.Model;

namespace Fission.DotNet
{
    public class SpecializeController : Controller
    {
        private readonly ILogger<SpecializeController> logger;
        private readonly ISpecializeService specializeService;
        
        public SpecializeController(ILogger<SpecializeController> logger, ISpecializeService specializeService)
        {
            this.logger = logger;
            this.specializeService = specializeService;        
        }

        // GET: SpecializeController
        [HttpPost, Route("/specialize")]
        public Task<object> Specialize()
        {
            logger.LogInformation("Specialize called");

            return Task.FromResult<object>(Results.Ok());
        }


        [HttpPost, Route("/v2/specialize")]
        public async Task<object> SpecializeV2()
        {
            logger.LogInformation("SpecializeV2 called");

            try
            {
                            

                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    logger.LogInformation($"Body: {body}");

                    var request = JsonSerializer.Deserialize<FissionSpecializeRequest>(body);
                    
                    specializeService.Specialize(request);

                    return Task.FromResult<object>(Results.Ok());
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error when specializing");
                return Task.FromResult<object>(Results.BadRequest(ex.Message));
            }
        }        
    }
}
