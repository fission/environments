#region header

// fission-dotnet6 - Startup.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/28 11:19 PM.

#endregion

#region using

using JetBrains.Annotations;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#endregion

namespace Fission.DotNet
{
    /// <summary>
    ///     Configure the environment container's web interface.
    /// </summary>
    public class Startup
    {
        [UsedImplicitly]
        public Startup (IConfiguration configuration) => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices (IServiceCollection services)
        {
            services.AddSingleton<IFunctionStore, FunctionStore>();
            services.AddControllers();
            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        public void Configure ([NotNull] IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints (configure: endpoints => { endpoints.MapControllers(); });
        }
    }
}
