using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace KidGuard.Client.Api;

public sealed class PairCodeApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PairCodeSession> CreatePairCodeAsync(
        Uri apiBaseUrl,
        string deviceName,
        string computerName,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = apiBaseUrl
        };

        using var response = await httpClient.PostAsJsonAsync(
            "pairing/child/connection-code",
            new CreatePairCodeRequest(deviceName, computerName, "1.0.1"),
            JsonOptions,
            cancellationToken);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<CreatePairCodeResponse>>(
            JsonOptions,
            cancellationToken);

        if (apiResponse is null)
        {
            throw new InvalidOperationException("Backend returned an empty response.");
        }

        if (!response.IsSuccessStatusCode || !apiResponse.Success || apiResponse.Data is null)
        {
            var errorDetails = apiResponse.Errors is { Count: > 0 }
                ? string.Join(", ", apiResponse.Errors)
                : response.StatusCode.ToString();

            throw new InvalidOperationException($"{apiResponse.Message} ({errorDetails})");
        }

        return new PairCodeSession(
            apiResponse.Data.DeviceId,
            apiResponse.Data.ConnectionCode,
            apiResponse.Data.ExpiresInSeconds,
            apiResponse.Data.ExpiresAt);
    }

    private sealed record CreatePairCodeRequest(string DeviceName, string ComputerName, string AgentVersion);

    private sealed record CreatePairCodeResponse(
        Guid DeviceId,
        string ConnectionCode,
        int ExpiresInSeconds,
        DateTime ExpiresAt);
}
