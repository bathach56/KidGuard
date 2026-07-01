namespace KidGuard.Client.Api;

public sealed record AuthSession(string AccessToken, int ExpiresIn);
