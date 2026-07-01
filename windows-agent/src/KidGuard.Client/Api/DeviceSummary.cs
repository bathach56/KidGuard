namespace KidGuard.Client.Api;

public sealed record DeviceSummary(
    Guid DeviceId,
    string DeviceName,
    string Mode,
    bool IsOnline,
    DateTime? LastSeen)
{
    public string DisplayText
    {
        get
        {
            var onlineStatus = IsOnline ? "Online" : "Offline";
            var lastSeenText = LastSeen.HasValue
                ? $"Last seen: {LastSeen.Value.ToLocalTime():yyyy-MM-dd HH:mm}"
                : "Last seen: never";

            return $"{DeviceName} | {Mode} | {onlineStatus} | {lastSeenText}";
        }
    }
}
