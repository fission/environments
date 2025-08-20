using System.ComponentModel;
using System.Text.Json;
using Fission.DotNet;
using Fission.DotNet.Adapter;
using Fission.DotNet.Interfaces;
using Fission.DotNet.Services;

var builder = WebApplication.CreateBuilder(args);

// Use port 8888 for runtime
builder.WebHost.UseUrls("http://*:8888");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddSingleton<ISpecializeService, SpecializeService>();
builder.Services.AddTransient<IFunctionService, FunctionService>();
builder.Services.AddSingleton<IFunctionStoreService, FunctionStoreService>();

// Add the builder mode flag as a singleton service so controllers can access it
builder.Services.AddSingleton<BuilderModeService>(new BuilderModeService(false));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting(); // Add the routing middleware

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
