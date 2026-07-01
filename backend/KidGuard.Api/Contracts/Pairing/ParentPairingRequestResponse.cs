namespace KidGuard.Api.Contracts.Pairing;

public record ParentPairingRequestResponse(
    Guid PairingRequestId,
    Guid DeviceId,
    string DeviceName,
    string ComputerName,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);
