using System.ComponentModel.DataAnnotations;

namespace KidGuard.Api.Contracts.Devices;

public record UpdateDeviceModeRequest([property: Required] string Mode);
