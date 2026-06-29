using System.ComponentModel.DataAnnotations;

namespace KidGuard.Api.Contracts.PairCode;

public record CreatePairCodeRequest(
    [property: Required, MaxLength(100)] string DeviceName,
    [property: Required, MaxLength(100)] string ComputerName,
    [property: Required, MaxLength(20)] string AgentVersion);
