using System.Net.Http.Headers;
using System.Net.Http.Json;
using KidGuard.Agent.Configuration;
using KidGuard.Agent.Models;
using Microsoft.Extensions.Options;

namespace KidGuard.Agent.Infrastructure;

public sealed class BackendApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptionsMonitor<AgentOptions> _options;
    private readonly ILogger<BackendApiClient> _logger;

    public BackendApiClient(
        HttpClient httpClient,
        IOptionsMonitor<AgentOptions> options,
        ILogger<BackendApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    public async Task<PairCodeData?> CreatePairCodeAsync(CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        if (!TryPrepareClient(options.ApiBaseUrl, options.SetupToken))
        {
            return null;
        }

        var request = new CreatePairCodeRequest(
            options.DeviceName,
            Environment.MachineName,
            options.AgentVersion);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "pair-code",
                request,
                BackendApiJsonContext.Default.CreatePairCodeRequest,
                cancellationToken);

            var apiResponse = await response.Content.ReadFromJsonAsync(
                BackendApiJsonContext.Default.ApiResponsePairCodeData,
                cancellationToken);

            return response.IsSuccessStatusCode && apiResponse?.Success == true
                ? apiResponse.Data
                : null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Failed to create pair code.");
            return null;
        }
    }

    public async Task<int?> SendHeartbeatAsync(CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        if (!TryPrepareDeviceClient(options))
        {
            return null;
        }

        var request = new HeartbeatRequest("online", options.AgentVersion);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"devices/{options.DeviceId}/heartbeat",
                request,
                BackendApiJsonContext.Default.HeartbeatRequest,
                cancellationToken);

            var apiResponse = await response.Content.ReadFromJsonAsync(
                BackendApiJsonContext.Default.ApiResponseHeartbeatData,
                cancellationToken);

            return response.IsSuccessStatusCode && apiResponse?.Success == true
                ? apiResponse.Data?.NextHeartbeat
                : null;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Failed to send heartbeat.");
            return null;
        }
    }

    public async Task<AgentMode?> GetCurrentModeAsync(CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        if (!TryPrepareDeviceClient(options))
        {
            return null;
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"devices/{options.DeviceId}/mode",
                cancellationToken);

            var apiResponse = await response.Content.ReadFromJsonAsync(
                BackendApiJsonContext.Default.ApiResponseModeData,
                cancellationToken);

            if (!response.IsSuccessStatusCode || apiResponse?.Success != true || apiResponse.Data is null)
            {
                return null;
            }

            return ParseMode(apiResponse.Data.Mode);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Failed to synchronize mode.");
            return null;
        }
    }

    public async Task<bool> UploadLogAsync(ActivityLogEntry logEntry, CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        if (!TryPrepareDeviceClient(options))
        {
            return false;
        }

        var request = new UploadLogRequest(
            logEntry.ProcessName,
            logEntry.Action,
            ToApiMode(logEntry.Mode),
            logEntry.Message);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"devices/{options.DeviceId}/logs",
                request,
                BackendApiJsonContext.Default.UploadLogRequest,
                cancellationToken);

            var apiResponse = await response.Content.ReadFromJsonAsync(
                BackendApiJsonContext.Default.ApiResponseObject,
                cancellationToken);

            return response.IsSuccessStatusCode && apiResponse?.Success == true;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Failed to upload activity log.");
            return false;
        }
    }

    private bool TryPrepareDeviceClient(AgentOptions options)
    {
        return TryPrepareClient(options.ApiBaseUrl, options.DeviceToken)
            && !string.IsNullOrWhiteSpace(options.DeviceId);
    }

    private bool TryPrepareClient(string apiBaseUrl, string bearerToken)
    {
        if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var baseUri)
            || string.IsNullOrWhiteSpace(bearerToken))
        {
            return false;
        }

        _httpClient.BaseAddress = baseUri;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return true;
    }

    private static AgentMode? ParseMode(string mode)
    {
        return mode switch
        {
            "fun" => AgentMode.Fun,
            "study" => AgentMode.Study,
            "punishment" => AgentMode.Punishment,
            _ => null
        };
    }

    public static string ToApiMode(AgentMode mode)
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
