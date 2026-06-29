using KidGuard.Agent.Configuration;
using KidGuard.Agent.Infrastructure;
using KidGuard.Agent.Services;
using KidGuard.Agent.Windows;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
var isCommandMode = AgentCommandRunner.IsCommandMode(args);

if (!isCommandMode)
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "KidGuardAgent";
    });
}

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services
    .AddOptions<AgentOptions>()
    .Bind(builder.Configuration.GetSection(AgentOptions.SectionName));

builder.Services.AddSingleton<DeviceCredentialStore>();

if (isCommandMode)
{
    builder.Services.AddSingleton<AgentCommandRunner>();
}
else
{
    builder.Services.AddSingleton<IValidateOptions<AgentOptions>, AgentOptionsValidator>();
    builder.Services.AddOptions<AgentOptions>().ValidateOnStart();
    builder.Services
        .AddHttpClient<BackendApiClient>()
        .AddPolicyHandler(GetHttpRetryPolicy());
    builder.Services.AddSingleton<LocalCacheService>();
    builder.Services.AddSingleton<PairingService>();
    builder.Services.AddSingleton<ProcessRuleProvider>();
    builder.Services.AddSingleton<ProcessBlockerService>();
    builder.Services.AddSingleton<ProcessMonitorService>();
    builder.Services.AddHostedService<AgentWorker>();
}

var host = builder.Build();

if (isCommandMode)
{
    var commandRunner = host.Services.GetRequiredService<AgentCommandRunner>();
    Environment.ExitCode = await commandRunner.RunAsync(args, CancellationToken.None);
    return;
}

await host.RunAsync();

static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
