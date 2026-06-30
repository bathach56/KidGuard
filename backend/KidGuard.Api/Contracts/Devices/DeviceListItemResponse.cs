namespace KidGuard.Api.Contracts.Devices;

public record DeviceListItemResponse(
    Guid DeviceId,
    string DeviceName,
    string Mode,
    bool IsOnline,
    DateTime? LastSeen);
