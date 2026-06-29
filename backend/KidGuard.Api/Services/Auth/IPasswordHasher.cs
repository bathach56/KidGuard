namespace KidGuard.Api.Services.Auth;

public interface IPasswordHasher
{
    bool Verify(string password, string passwordHash);
}
