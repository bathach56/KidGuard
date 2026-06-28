using KidGuard.Agent.Configuration;
using KidGuard.Agent.Infrastructure;
using Microsoft.Extensions.Options;

namespace KidGuard.Agent.Services;

public sealed class AgentWorker : BackgroundService
{
    private readonly BackendApiClient _backendApiClient;
    private readonly LocalCacheService _localCacheService;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly IOptionsMonitor<AgentOptions> _options;
    private readonly ILogger<AgentWorker> _logger;
    private DateTimeOffset _nextHeartbeatAt = DateTimeOffset.MinValue;
    private DateTimeOffset _nextModeSyncAt = DateTimeOffset.MinValue;
    private DateTimeOffset _nextProcessScanAt = DateTimeOffset.MinValue;

    public AgentWorker(
        BackendApiClient backendApiClient,
        LocalCacheService localCacheService,
        ProcessMonitorService processMonitorService,
        IOptionsMonitor<AgentOptions> options,
        ILogger<AgentWorker> logger)
    {
        _backendApiClient = backendApiClient;
        _localCacheService = localCacheService;
        _processMonitorService = processMonitorService;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("KidGuard Agent started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                await RunDueWorkAsync(now, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected agent worker error.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("KidGuard Agent stopped.");
    }

    private async Task RunDueWorkAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (now >= _nextHeartbeatAt)
        {
            await SendHeartbeatAsync(cancellationToken);
            _nextHeartbeatAt = now.AddSeconds(_options.CurrentValue.HeartbeatIntervalSeconds);
        }

        if (now >= _nextModeSyncAt)
        {
            await SyncModeAsync(cancellationToken);
            _nextModeSyncAt = now.AddSeconds(_options.CurrentValue.ModeSyncIntervalSeconds);
        }

        if (now >= _nextProcessScanAt)
        {
            await _processMonitorService.ScanAsync(cancellationToken);
            _nextProcessScanAt = now.AddSeconds(_options.CurrentValue.ProcessScanIntervalSeconds);
        }

        await UploadPendingLogsAsync(cancellationToken);
    }

    private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        var nextHeartbeat = await _backendApiClient.SendHeartbeatAsync(cancellationToken);
        if (nextHeartbeat is not null)
        {
            _logger.LogInformation("Heartbeat sent. Backend requested next heartbeat in {Seconds} seconds.", nextHeartbeat);
        }
    }

    private async Task SyncModeAsync(CancellationToken cancellationToken)
    {
        var mode = await _backendApiClient.GetCurrentModeAsync(cancellationToken);
        if (mode is null)
        {
            return;
        }

        await _localCacheService.UpdateModeAsync(mode.Value, cancellationToken);
        _logger.LogInformation("Mode synchronized: {Mode}.", mode);
    }

    private async Task UploadPendingLogsAsync(CancellationToken cancellationToken)
    {
        var pendingLogs = await _localCacheService.GetPendingLogsAsync(cancellationToken);
        var uploadedLogs = new List<Models.ActivityLogEntry>();

        foreach (var logEntry in pendingLogs)
        {
            var uploaded = await _backendApiClient.UploadLogAsync(logEntry, cancellationToken);
            if (uploaded)
            {
                uploadedLogs.Add(logEntry);
            }
        }

        await _localCacheService.RemovePendingLogsAsync(uploadedLogs, cancellationToken);
    }
}
