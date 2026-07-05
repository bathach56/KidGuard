using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace KidGuard.Client.Api;

public sealed class AuthApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task RegisterAsync(
        Uri apiBaseUrl,
        string email,
        string password,
        string fullName,
        string? phoneNumber,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = apiBaseUrl
        };

        using var response = await httpClient.PostAsJsonAsync(
            "auth/register",
            new RegisterRequest(email, password, fullName, string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber),
            JsonOptions,
            cancellationToken);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<RegisterResponse>>(
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
    }

    public async Task<AuthSession> LoginAsync(
        Uri apiBaseUrl,
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient
        {
            BaseAddress = apiBaseUrl
        };

        using var response = await httpClient.PostAsJsonAsync(
            "auth/login",
            new LoginRequest(email, password),
            JsonOptions,
            cancellationToken);

        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>(
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

        return new AuthSession(apiResponse.Data.AccessToken, apiResponse.Data.ExpiresIn);
    }

    private sealed record RegisterRequest(string Email, string Password, string FullName, string? PhoneNumber);

    private sealed record RegisterResponse(Guid UserId, string FullName, string Email, string? PhoneNumber);

    private sealed record LoginRequest(string Email, string Password);

    private sealed record LoginResponse(string AccessToken, int ExpiresIn);
}
