using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KidGuard.Client.Services;

[SupportedOSPlatform("windows")]
public sealed class ClientDeviceCredentialStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("KidGuard.Agent.DeviceCredentials");
    private const string CredentialFileName = "device-credentials.dat";

    public async Task SaveCredentialsAsync(
        Guid deviceId,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(GetCredentialDirectory());

        var credentialJsonBytes = JsonSerializer.SerializeToUtf8Bytes(new DeviceCredentials(
            deviceId.ToString(),
            deviceToken));
        var protectedBytes = ProtectedData.Protect(
            credentialJsonBytes,
            Entropy,
            DataProtectionScope.LocalMachine);

        await File.WriteAllBytesAsync(GetCredentialPath(), protectedBytes, cancellationToken);
    }

    private static string GetCredentialPath()
    {
        return Path.Combine(GetCredentialDirectory(), CredentialFileName);
    }

    private static string GetCredentialDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KidGuard",
            "Agent");
    }

    private sealed record DeviceCredentials(string DeviceId, string DeviceToken);
}
