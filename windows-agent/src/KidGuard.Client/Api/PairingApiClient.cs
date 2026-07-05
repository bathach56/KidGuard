using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace KidGuard.Client.Api;

public sealed class PairingApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PairingRequestSession> CreateParentPairingRequestAsync(
        Uri apiBaseUrl,
        string accessToken,
        string connectionCode,
        CancellationToken cancellationToken)
    {
        using var httpClient = CreateJsonClient(apiBaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.PostAsJsonAsync(
            "pairing/requests",
            new CreateParentPairingRequest(connectionCode),
            JsonOptions,
            cancellationToken);

        var apiResponse = await ReadApiResponseAsync<ParentPairingRequestResponse>(response, cancellationToken);
        return new PairingRequestSession(
            apiResponse.PairingRequestId,
            apiResponse.DeviceId,
            apiResponse.DeviceName,
            apiResponse.ComputerName,
            apiResponse.Status,
            apiResponse.ExpiresAt,
            apiResponse.CreatedAt);
    }

    public async Task<PairingRequestSession> GetPairingStatusAsync(
        Uri apiBaseUrl,
        string accessToken,
        Guid pairingRequestId,
        CancellationToken cancellationToken)
    {
        using var httpClient = CreateJsonClient(apiBaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await httpClient.GetAsync(
            $"pairing/requests/{pairingRequestId}/status",
            cancellationToken);

        var apiResponse = await ReadApiResponseAsync<PairingStatusResponse>(response, cancellationToken);
        return new PairingRequestSession(
            apiResponse.PairingRequestId,
            apiResponse.DeviceId,
            apiResponse.DeviceName,
            apiResponse.ComputerName,
            apiResponse.Status,
            apiResponse.ExpiresAt,
            apiResponse.CreatedAt);
    }

    public async Task<PendingPairingRequest?> GetChildPendingRequestAsync(
        Uri apiBaseUrl,
        string connectionCode,
        CancellationToken cancellationToken)
    {
        using var httpClient = CreateJsonClient(apiBaseUrl);

        using var response = await httpClient.GetAsync(
            $"pairing/child/pending?connectionCode={Uri.EscapeDataString(connectionCode)}",
            cancellationToken);

        var apiResponse = await ReadNullableApiResponseAsync<ChildPendingPairingRequestResponse>(response, cancellationToken);
        return apiResponse is null
            ? null
            : new PendingPairingRequest(
                apiResponse.PairingRequestId,
                apiResponse.DeviceId,
                apiResponse.DeviceName,
                apiResponse.ComputerName,
                apiResponse.ParentFullName,
                apiResponse.ParentEmail,
                apiResponse.Status,
                apiResponse.ExpiresAt,
                apiResponse.CreatedAt);
    }

    public async Task<PairingDecisionResult> ApprovePairingAsync(
        Uri apiBaseUrl,
        Guid pairingRequestId,
        string connectionCode,
        CancellationToken cancellationToken)
    {
        using var httpClient = CreateJsonClient(apiBaseUrl);

        using var response = await httpClient.PostAsJsonAsync(
            "pairing/child/approve",
            new ChildPairingDecisionRequest(pairingRequestId, connectionCode),
            JsonOptions,
            cancellationToken);

        var apiResponse = await ReadApiResponseAsync<ChildApprovePairingResponse>(response, cancellationToken);
        return new PairingDecisionResult(
            apiResponse.PairingRequestId,
            apiResponse.DeviceId,
            apiResponse.DeviceName,
            apiResponse.CurrentMode,
            apiResponse.Status,
            apiResponse.DeviceToken);
    }

    public async Task<PairingDecisionResult> RejectPairingAsync(
        Uri apiBaseUrl,
        Guid pairingRequestId,
        string connectionCode,
        CancellationToken cancellationToken)
    {
        using var httpClient = CreateJsonClient(apiBaseUrl);

        using var response = await httpClient.PostAsJsonAsync(
            "pairing/child/reject",
            new ChildPairingDecisionRequest(pairingRequestId, connectionCode),
            JsonOptions,
            cancellationToken);

        var apiResponse = await ReadApiResponseAsync<ChildRejectPairingResponse>(response, cancellationToken);
        return new PairingDecisionResult(
            apiResponse.PairingRequestId,
            null,
            null,
            null,
            apiResponse.Status,
            null);
    }

    private static HttpClient CreateJsonClient(Uri apiBaseUrl)
    {
        return new HttpClient
        {
            BaseAddress = apiBaseUrl
        };
    }

    private static async Task<T> ReadApiResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(
            JsonOptions,
            cancellationToken);

        if (apiResponse is null)
        {
            throw new InvalidOperationException("Backend returned an empty response.");
        }

        if (!apiResponse.Success || apiResponse.Data is null)
        {
            throw new InvalidOperationException(BuildErrorMessage(response, apiResponse.Message, apiResponse.Errors));
        }

        return apiResponse.Data;
    }

    private static async Task<T?> ReadNullableApiResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T?>>(
            JsonOptions,
            cancellationToken);

        if (apiResponse is null)
        {
            throw new InvalidOperationException("Backend returned an empty response.");
        }

        if (!apiResponse.Success)
        {
            throw new InvalidOperationException(BuildErrorMessage(response, apiResponse.Message, apiResponse.Errors));
        }

        return apiResponse.Data;
    }

    private static string BuildErrorMessage(
        HttpResponseMessage response,
        string message,
        IReadOnlyList<string>? errors)
    {
        var errorDetails = errors is { Count: > 0 }
            ? string.Join(", ", errors)
            : response.StatusCode.ToString();

        return $"{message} ({errorDetails})";
    }

    private sealed record CreateParentPairingRequest(string ConnectionCode);

    private sealed record ChildPairingDecisionRequest(Guid PairingRequestId, string ConnectionCode);

    private sealed record ParentPairingRequestResponse(
        Guid PairingRequestId,
        Guid DeviceId,
        string DeviceName,
        string ComputerName,
        string Status,
        DateTime ExpiresAt,
        DateTime CreatedAt);

    private sealed record PairingStatusResponse(
        Guid PairingRequestId,
        Guid DeviceId,
        string DeviceName,
        string ComputerName,
        string Status,
        DateTime ExpiresAt,
        DateTime CreatedAt,
        DateTime? ApprovedAt,
        DateTime? RejectedAt);

    private sealed record ChildPendingPairingRequestResponse(
        Guid PairingRequestId,
        Guid DeviceId,
        string DeviceName,
        string ComputerName,
        string ParentFullName,
        string ParentEmail,
        string Status,
        DateTime ExpiresAt,
        DateTime CreatedAt);

    private sealed record ChildApprovePairingResponse(
        Guid PairingRequestId,
        Guid DeviceId,
        string DeviceName,
        string CurrentMode,
        string Status,
        string DeviceToken);

    private sealed record ChildRejectPairingResponse(
        Guid PairingRequestId,
        string Status,
        DateTime RejectedAt);
}
