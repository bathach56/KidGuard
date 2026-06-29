using System.ComponentModel.DataAnnotations;

namespace KidGuard.Api.Contracts.Devices;

public record HeartbeatRequest(
    [property: Required, MaxLength(20)] string Status,
    [property: Required, MaxLength(20)] string AgentVersion);
