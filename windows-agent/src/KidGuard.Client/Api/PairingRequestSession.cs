namespace KidGuard.Client.Api;

public sealed record PairingRequestSession(
    Guid PairingRequestId,
    Guid DeviceId,
    string DeviceName,
    string ComputerName,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt)
{
    public string DisplayText => $"{DeviceName} ({ComputerName}) - {Status}";
}
