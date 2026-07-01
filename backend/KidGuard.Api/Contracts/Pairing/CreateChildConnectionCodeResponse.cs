namespace KidGuard.Api.Contracts.Pairing;

public record CreateChildConnectionCodeResponse(
    Guid DeviceId,
    string ConnectionCode,
    int ExpiresInSeconds,
    DateTime ExpiresAt);
