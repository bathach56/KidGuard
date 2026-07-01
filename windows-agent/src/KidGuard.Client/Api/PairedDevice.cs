namespace KidGuard.Client.Api;

public sealed record PairedDevice(
    Guid DeviceId,
    string DeviceName,
    string Mode,
    string DeviceToken);
