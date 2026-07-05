namespace KidGuard.Client.Api;

public sealed record PairingDecisionResult(
    Guid PairingRequestId,
    Guid? DeviceId,
    string? DeviceName,
    string? CurrentMode,
    string Status,
    string? DeviceToken);
