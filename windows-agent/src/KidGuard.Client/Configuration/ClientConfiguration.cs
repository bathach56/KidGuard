namespace KidGuard.Client.Configuration;

public static class ClientConfiguration
{
    public const string ApiBaseUrlEnvironmentVariable = "KIDGUARD_API_BASE_URL";
    public const string SetupTokenEnvironmentVariable = "KIDGUARD_SETUP_TOKEN";
    public const string AgentSetupTokenEnvironmentVariable = "Agent__SetupToken";

    public static string GetConfiguredApiBaseUrl()
    {
        return Environment.GetEnvironmentVariable(ApiBaseUrlEnvironmentVariable)?.Trim() ?? string.Empty;
    }

    public static string GetConfiguredSetupToken()
    {
        var clientToken = Environment.GetEnvironmentVariable(SetupTokenEnvironmentVariable)?.Trim();
        if (!string.IsNullOrWhiteSpace(clientToken))
        {
            return clientToken;
        }

        return Environment.GetEnvironmentVariable(AgentSetupTokenEnvironmentVariable)?.Trim() ?? string.Empty;
    }
}
