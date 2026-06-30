namespace KidGuard.Api.Contracts.Devices;

public record PairDeviceResponse(
    Guid DeviceId,
    string DeviceName,
    string Mode,
    string DeviceToken);
