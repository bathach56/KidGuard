using System.Text.Json;
using KidGuard.Agent.Configuration;
using KidGuard.Agent.Models;
using Microsoft.Extensions.Options;

namespace KidGuard.Agent.Services;

public sealed class LocalCacheService
{
    private readonly IOptionsMonitor<AgentOptions> _options;
    private readonly ILogger<LocalCacheService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private AgentCache? _cache;

    public LocalCacheService(
        IOptionsMonitor<AgentOptions> options,
        ILogger<LocalCacheService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<AgentCache> GetCacheAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache is not null)
            {
                return _cache;
            }

            var path = GetCachePath();
            if (!File.Exists(path))
            {
                _cache = new AgentCache();
                return _cache;
            }

            await using var stream = File.OpenRead(path);
            _cache = await JsonSerializer.DeserializeAsync<AgentCache>(
                stream,
                cancellationToken: cancellationToken) ?? new AgentCache();

            return _cache;
        }
        catch (Exception exception) when (exception is IOException or JsonException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Failed to read local cache.");
            _cache = new AgentCache();
            return _cache;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateModeAsync(AgentMode mode, CancellationToken cancellationToken)
    {
        var cache = await GetCacheAsync(cancellationToken);
        cache.CurrentMode = mode;
        cache.LastSuccessfulSyncAt = DateTimeOffset.UtcNow;
        await SaveAsync(cache, cancellationToken);
    }

    public async Task AddPendingLogAsync(ActivityLogEntry logEntry, CancellationToken cancellationToken)
    {
        var cache = await GetCacheAsync(cancellationToken);
        cache.PendingLogs.Add(logEntry);
        await SaveAsync(cache, cancellationToken);
    }

    public async Task<IReadOnlyList<ActivityLogEntry>> GetPendingLogsAsync(CancellationToken cancellationToken)
    {
        var cache = await GetCacheAsync(cancellationToken);
        return cache.PendingLogs.ToArray();
    }

    public async Task RemovePendingLogsAsync(
        IReadOnlyCollection<ActivityLogEntry> uploadedLogs,
        CancellationToken cancellationToken)
    {
        if (uploadedLogs.Count == 0)
        {
            return;
        }

        var cache = await GetCacheAsync(cancellationToken);
        cache.PendingLogs = cache.PendingLogs
            .Where(logEntry => !uploadedLogs.Contains(logEntry))
            .ToList();

        await SaveAsync(cache, cancellationToken);
    }

    private async Task SaveAsync(AgentCache cache, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(GetCacheDirectory());
            await using var stream = File.Create(GetCachePath());
            await JsonSerializer.SerializeAsync(stream, cache, cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(exception, "Failed to write local cache.");
        }
        finally
        {
            _lock.Release();
        }
    }

    private string GetCachePath()
    {
        return Path.Combine(GetCacheDirectory(), _options.CurrentValue.LocalCacheFileName);
    }

    private static string GetCacheDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KidGuard",
            "Agent");
    }
}
