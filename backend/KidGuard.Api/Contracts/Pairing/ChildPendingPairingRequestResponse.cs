namespace KidGuard.Api.Contracts.Pairing;

public record ChildPendingPairingRequestResponse(
    Guid PairingRequestId,
    Guid DeviceId,
    string DeviceName,
    string ComputerName,
    string ParentFullName,
    string ParentEmail,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);
