namespace KidGuard.Api.Services.Auth;

public class BCryptPasswordHasher : IPasswordHasher
{
    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        return global::BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}

