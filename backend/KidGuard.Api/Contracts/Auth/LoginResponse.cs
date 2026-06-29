namespace KidGuard.Api.Contracts.Auth;

public record LoginResponse(string AccessToken, int ExpiresIn);
