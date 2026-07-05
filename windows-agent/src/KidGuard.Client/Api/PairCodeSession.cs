namespace KidGuard.Client.Api;

public sealed record PairCodeSession(
    Guid DeviceId,
    string ConnectionCode,
    int ExpiresInSeconds,
    DateTime ExpiresAt);
