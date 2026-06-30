using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;

namespace KidGuard.Api.Services.Pairing;

public class DeviceTokenGenerator : IDeviceTokenGenerator
{
    private const int TokenByteLength = 32;

    public string Generate()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenByteLength));
    }
}

