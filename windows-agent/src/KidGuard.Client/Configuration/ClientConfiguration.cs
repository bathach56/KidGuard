namespace KidGuard.Client.Configuration;

public static class ClientConfiguration
{
    public const string ApiBaseUrlEnvironmentVariable = "KIDGUARD_API_BASE_URL";

    public static string GetConfiguredApiBaseUrl()
    {
        return Environment.GetEnvironmentVariable(ApiBaseUrlEnvironmentVariable)?.Trim() ?? string.Empty;
    }
}
