namespace KidGuard.Api.Contracts.Devices;

public record DeviceDetailResponse(
    Guid DeviceId,
    string DeviceName,
    string Mode,
    bool IsOnline,
    DateTime? LastSeen);
