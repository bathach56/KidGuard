using KidGuard.Agent.Configuration;
using KidGuard.Agent.Infrastructure;
using KidGuard.Agent.Services;
using KidGuard.Agent.Windows;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "KidGuard Agent";
});

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services
    .AddOptions<AgentOptions>()
    .Bind(builder.Configuration.GetSection(AgentOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddSingleton<IValidateOptions<AgentOptions>, AgentOptionsValidator>();
builder.Services
    .AddHttpClient<BackendApiClient>()
    .AddPolicyHandler(GetHttpRetryPolicy());
builder.Services.AddSingleton<LocalCacheService>();
builder.Services.AddSingleton<DeviceCredentialStore>();
builder.Services.AddSingleton<PairingService>();
builder.Services.AddSingleton<ProcessRuleProvider>();
builder.Services.AddSingleton<ProcessBlockerService>();
builder.Services.AddSingleton<ProcessMonitorService>();
builder.Services.AddHostedService<AgentWorker>();

var host = builder.Build();
host.Run();

static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
