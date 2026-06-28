namespace KidGuard.Agent.Configuration;

public sealed class AgentOptions
{
    public const string SectionName = "Agent";

    public string ApiBaseUrl { get; set; } = string.Empty;

    public string SetupToken { get; set; } = string.Empty;

    public string DeviceId { get; set; } = string.Empty;

    public string DeviceToken { get; set; } = string.Empty;

    public string DeviceName { get; set; } = "Study PC";

    public string AgentVersion { get; set; } = "1.0.0";

    public int HeartbeatIntervalSeconds { get; set; } = 30;

    public int ModeSyncIntervalSeconds { get; set; } = 10;

    public int ProcessScanIntervalSeconds { get; set; } = 3;

    public string LocalCacheFileName { get; set; } = "agent-cache.json";
}
