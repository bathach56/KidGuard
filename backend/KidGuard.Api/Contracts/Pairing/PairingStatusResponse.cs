namespace KidGuard.Api.Contracts.Pairing;

public record PairingStatusResponse(
    Guid PairingRequestId,
    Guid DeviceId,
    string DeviceName,
    string ComputerName,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    DateTime? RejectedAt);
