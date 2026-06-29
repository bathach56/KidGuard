using System.Security.Cryptography;

namespace KidGuard.Api.Services.Pairing;

public class PairCodeGenerator : IPairCodeGenerator
{
    private const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int PairCodeLength = 8;

    public string Generate()
    {
        return RandomNumberGenerator.GetString(AllowedCharacters, PairCodeLength);
    }
}
