using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace KidGuard.Client.Api;

public sealed class PairCodeApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PairCodeSession> CreatePairCodeAsync(
        Uri apiBaseUrl,
        string setupToken,
        string deviceName,
        string computerName,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = apiBaseUrl
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", setupToken);

        using var response = await httpClient.PostAsJsonAsync(
            "pair-code",
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

        return new PairCodeSession(apiResponse.Data.PairCode, apiResponse.Data.ExpiresIn);
    }

    private sealed record CreatePairCodeRequest(string DeviceName, string ComputerName, string AgentVersion);

    private sealed record CreatePairCodeResponse(string PairCode, int ExpiresIn);
}
