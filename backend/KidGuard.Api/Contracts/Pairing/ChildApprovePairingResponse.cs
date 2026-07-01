namespace KidGuard.Api.Contracts.Pairing;

public record ChildApprovePairingResponse(
    Guid PairingRequestId,
    Guid DeviceId,
    string DeviceName,
    string CurrentMode,
    string Status,
    string DeviceToken);
