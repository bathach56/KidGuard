using System.Diagnostics;
using KidGuard.Agent.Models;
using KidGuard.Agent.Windows;

namespace KidGuard.Agent.Services;

public sealed class ProcessMonitorService
{
    private readonly ProcessRuleProvider _processRuleProvider;
    private readonly ProcessBlockerService _processBlockerService;
    private readonly LocalCacheService _localCacheService;
    private readonly ILogger<ProcessMonitorService> _logger;

    public ProcessMonitorService(
        ProcessRuleProvider processRuleProvider,
        ProcessBlockerService processBlockerService,
        LocalCacheService localCacheService,
        ILogger<ProcessMonitorService> logger)
    {
        _processRuleProvider = processRuleProvider;
        _processBlockerService = processBlockerService;
        _localCacheService = localCacheService;
        _logger = logger;
    }

    public async Task ScanAsync(CancellationToken cancellationToken)
    {
        var cache = await _localCacheService.GetCacheAsync(cancellationToken);

        foreach (var process in Process.GetProcesses())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var processName = GetProcessName(process);

            if (!_processRuleProvider.ShouldBlock(processName, cache.CurrentMode))
            {
                continue;
            }

            var blocked = _processBlockerService.TryBlock(process, processName);
            if (!blocked)
            {
                continue;
            }

            var logEntry = new ActivityLogEntry
            {
                ProcessName = processName,
                Action = "blocked",
                Mode = cache.CurrentMode,
                Message = $"Blocked by {BackendApiClientModeName(cache.CurrentMode)} mode."
            };

            await _localCacheService.AddPendingLogAsync(logEntry, cancellationToken);
            _logger.LogInformation("Blocked process {ProcessName} in {Mode} mode.", processName, cache.CurrentMode);
        }
    }

    private static string GetProcessName(Process process)
    {
        try
        {
            return process.ProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? process.ProcessName
                : $"{process.ProcessName}.exe";
        }
        catch (InvalidOperationException)
        {
            return string.Empty;
        }
    }

    private static string BackendApiClientModeName(AgentMode mode)
    {
        return mode switch
        {
            AgentMode.Fun => "fun",
            AgentMode.Study => "study",
            AgentMode.Punishment => "punishment",
            _ => "fun"
        };
    }
}
