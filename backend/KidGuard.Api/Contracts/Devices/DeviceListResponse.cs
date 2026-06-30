namespace KidGuard.Api.Contracts.Devices;

public record DeviceListResponse(IReadOnlyList<DeviceListItemResponse> Items);
