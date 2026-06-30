using System.ComponentModel.DataAnnotations;

namespace KidGuard.Api.Contracts.Devices;

public record PairDeviceRequest([property: Required] string PairCode);
