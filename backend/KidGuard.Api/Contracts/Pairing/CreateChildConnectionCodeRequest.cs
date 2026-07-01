namespace KidGuard.Api.Contracts.Pairing;

public record CreateChildConnectionCodeRequest(
    string DeviceName,
    string ComputerName,
    string AgentVersion);
