using System.ComponentModel.DataAnnotations;

namespace KidGuard.Api.Contracts.Devices;

public record UploadDeviceLogRequest(
    [property: Required, MaxLength(255)] string ProcessName,
    [property: Required, MaxLength(100)] string Action,
    [property: Required, MaxLength(20)] string Mode,
    [property: Required] string Message);
