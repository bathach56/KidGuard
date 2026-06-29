namespace KidGuard.Api.Contracts.Devices;

public record DeviceLogsResponse(
    IReadOnlyList<DeviceLogItemResponse> Items,
    int Total);
