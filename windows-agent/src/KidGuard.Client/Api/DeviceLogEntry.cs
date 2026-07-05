namespace KidGuard.Client.Api;

public sealed record DeviceLogEntry(
    string ProcessName,
    string Action,
    string Mode,
    DateTime CreatedAt)
{
    public string DisplayText => $"{CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss} | {Mode} | {Action} | {ProcessName}";
}
