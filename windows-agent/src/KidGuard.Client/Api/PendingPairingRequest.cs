namespace KidGuard.Client.Api;

public sealed record PendingPairingRequest(
    Guid PairingRequestId,
    Guid DeviceId,
    string DeviceName,
    string ComputerName,
    string ParentFullName,
    string ParentEmail,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt)
{
    public string DisplayText => $"{ParentFullName} ({ParentEmail}) wants to manage {DeviceName}.";
}
