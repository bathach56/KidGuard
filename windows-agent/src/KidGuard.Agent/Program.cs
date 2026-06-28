using KidGuard.Agent.Configuration;
using KidGuard.Agent.Infrastructure;
using KidGuard.Agent.Services;
using KidGuard.Agent.Windows;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "KidGuard Agent";
});

builder.Services.Configure<AgentOptions>(
    builder.Configuration.GetSection(AgentOptions.SectionName));

builder.Services.AddHttpClient<BackendApiClient>();
builder.Services.AddSingleton<LocalCacheService>();
builder.Services.AddSingleton<ProcessRuleProvider>();
builder.Services.AddSingleton<ProcessBlockerService>();
builder.Services.AddSingleton<ProcessMonitorService>();
builder.Services.AddHostedService<AgentWorker>();

var host = builder.Build();
host.Run();
