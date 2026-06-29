namespace KidGuard.Api.Contracts.Devices;

public record DeviceLogItemResponse(
    string ProcessName,
    string Action,
    string Mode,
    DateTime CreatedAt);
