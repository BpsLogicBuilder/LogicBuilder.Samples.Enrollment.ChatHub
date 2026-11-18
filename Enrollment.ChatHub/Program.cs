using AutoMapper;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Enrollment.ChatHub;
using LogicBuilder.App.AI.Utils.Mapping;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

const string SecureCorsPolicy = "SecureCorsPolicy";

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy(SecureCorsPolicy, policy =>
    {
        if (allowedOrigins != null && allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .WithHeaders("Content-Type", "Authorization", "x-requested-with", "x-signalr-user-agent")
                  .AllowCredentials();
        }
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

builder.Services.AddOpenTelemetry()
    .WithTracing
    (
        tracing => tracing
            // Captures SignalR hub invocations and connection lifecycles
            .AddSource("Microsoft.AspNetCore.SignalR.Server")

            // Captures AIAgent runs, workflows, and tool executions
            .AddSource(Constants.AgentSourceName)
            .AddSource("Experimental.Microsoft.Agents.AI*")

            // Captures underlying IChatClient LLM calls and token metrics
            .AddSource("Microsoft.Extensions.AI*")
            .AddSource("OpenAI*")
    )
    .WithMetrics
    (
        metrics => metrics
            .AddMeter(Constants.AgentSourceName)
            .AddMeter("Microsoft.Extensions.AI*")
    )
    .UseAzureMonitor(options =>
    {
        options.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
    });

var app = builder.Build();

// Configure the HTTP request pipeline.

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

app.UseCors(SecureCorsPolicy);

// Map your WebSocket endpoints transparently
app.MapHub<AgentAIChatHub>("/agentChatHub");

await app.RunAsync();

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program() { }
}


