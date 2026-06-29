using KidGuard.Api.Entities;

namespace KidGuard.Api.Services.Auth;

public interface IJwtTokenService
{
    string CreateAccessToken(User user);
    int ExpiresInSeconds { get; }
}
