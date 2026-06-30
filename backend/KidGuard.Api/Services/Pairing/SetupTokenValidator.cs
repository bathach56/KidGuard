using System.Security.Cryptography;
using System.Text;
using KidGuard.Api.Options;
using Microsoft.Extensions.Options;

namespace KidGuard.Api.Services.Pairing;

public class SetupTokenValidator : ISetupTokenValidator
{
    private const string BearerPrefix = "Bearer ";
    private readonly SetupTokenSettings settings;

    public SetupTokenValidator(IOptions<SetupTokenSettings> options)
    {
        settings = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(settings.Token);

    public bool IsValid(string? authorizationHeader)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return false;
        }

        if (!authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var submittedToken = authorizationHeader[BearerPrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(submittedToken))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(settings.Token!);
        var submittedBytes = Encoding.UTF8.GetBytes(submittedToken);

        return expectedBytes.Length == submittedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, submittedBytes);
    }
}
