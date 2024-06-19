#region header

// fission-dotnet6 - Program.cs
// 
// Created by: Alistair J R Young (avatar) at 2020/12/28 11:19 PM.

#endregion

#region using

using Fission.DotNet;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

#endregion

IWebHost host = new WebHostBuilder()
               .ConfigureLogging (configureLogging: log => log.AddConsole())
               .UseKestrel()
               .UseUrls("http://*:8888")
               .UseStartup<Startup>()
               .Build();

host.Run();

return 0;
