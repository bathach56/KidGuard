using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace KidGuard.Client.Api;

public sealed class DeviceApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<DeviceSummary>> GetDevicesAsync(
        Uri apiBaseUrl,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = apiBaseUrl
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.GetAsync("devices", cancellationToken);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<DeviceListResponse>>(
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

        return apiResponse.Data.Items;
    }

    public async Task<PairedDevice> PairDeviceAsync(
        Uri apiBaseUrl,
        string accessToken,
        string pairCode,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = apiBaseUrl
        };
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.PostAsJsonAsync(
            "devices/pair",
            new PairDeviceRequest(pairCode),
            JsonOptions,
            cancellationToken);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PairDeviceResponse>>(
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

        return new PairedDevice(
            apiResponse.Data.DeviceId,
            apiResponse.Data.DeviceName,
            apiResponse.Data.Mode,
            apiResponse.Data.DeviceToken);
    }

    private sealed record PairDeviceRequest(string PairCode);

    private sealed record PairDeviceResponse(
        Guid DeviceId,
        string DeviceName,
        string Mode,
        string DeviceToken);

    private sealed record DeviceListResponse(IReadOnlyList<DeviceSummary> Items);
}
