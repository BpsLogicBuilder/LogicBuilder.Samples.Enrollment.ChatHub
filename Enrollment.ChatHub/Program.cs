using AutoMapper;
using LogicBuilder.App.AI.Utils.Mapping;
using Enrollment.ChatHub;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4205").AllowCredentials();
    });
});
builder.Services
    .AddSingleton<AutoMapper.IConfigurationProvider>
    (
        new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ParametersToBuilderMappingProfile>();
        }, new NullLoggerFactory())
    )
    .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
    .AddLogging()
    .AddHttpClient()
    .AddChatHubFlowServices()
    .AddSingleton<IAgentInitializer, AgentInitializer>();

// Configure the HTTP request pipeline.

var app = builder.Build();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.UseCors();

// Map your WebSocket endpoints transparently
app.MapHub<AgentAIChatHub>("/agentChatHub");

await app.RunAsync();

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program() { }
}


