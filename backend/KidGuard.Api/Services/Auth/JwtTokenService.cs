using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KidGuard.Api.Entities;
using KidGuard.Api.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KidGuard.Api.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        jwtSettings = jwtOptions.Value;
    }

    public int ExpiresInSeconds => jwtSettings.ExpiresInMinutes * 60;

    public string CreateAccessToken(User user)
    {
        if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            throw new InvalidOperationException("JWT secret is not configured.");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.ExpiresInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
