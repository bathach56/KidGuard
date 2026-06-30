namespace KidGuard.Api.Options;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "KidGuard.Api";
    public string Audience { get; set; } = "KidGuard.Mobile";
    public int ExpiresInMinutes { get; set; } = 60;
    public string? Secret { get; set; }
}
