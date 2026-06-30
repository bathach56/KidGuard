using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Runtime.Versioning;
using KidGuard.Agent.Configuration;
using KidGuard.Agent.Models;
using Microsoft.Extensions.Options;

namespace KidGuard.Agent.Services;

[SupportedOSPlatform("windows")]
public sealed class DeviceCredentialStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("KidGuard.Agent.DeviceCredentials");

    private readonly IOptionsMonitor<AgentOptions> _options;
    private readonly ILogger<DeviceCredentialStore> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DeviceCredentials? _cachedCredentials;

    public DeviceCredentialStore(
        IOptionsMonitor<AgentOptions> options,
        ILogger<DeviceCredentialStore> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<DeviceCredentials?> GetCredentialsAsync(CancellationToken cancellationToken)
    {
        var configuredCredentials = GetConfiguredCredentials();
        if (configuredCredentials is not null)
        {
            return configuredCredentials;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cachedCredentials is not null)
            {
                return _cachedCredentials;
            }

            var path = GetCredentialPath();
            if (!File.Exists(path))
            {
                return null;
            }

            var protectedBytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var credentialJsonBytes = ProtectedData.Unprotect(
                protectedBytes,
                Entropy,
                DataProtectionScope.LocalMachine);

            _cachedCredentials = JsonSerializer.Deserialize<DeviceCredentials>(credentialJsonBytes);
            return _cachedCredentials;
        }
        catch (Exception exception) when (
            exception is IOException
            or JsonException
            or CryptographicException
            or UnauthorizedAccessException
            or PlatformNotSupportedException)
        {
            _logger.LogError(exception, "Failed to read protected device credentials.");
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveCredentialsAsync(
        DeviceCredentials credentials,
        CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            Directory.CreateDirectory(GetCredentialDirectory());

            var credentialJsonBytes = JsonSerializer.SerializeToUtf8Bytes(credentials);
            var protectedBytes = ProtectedData.Protect(
                credentialJsonBytes,
                Entropy,
                DataProtectionScope.LocalMachine);

            await File.WriteAllBytesAsync(GetCredentialPath(), protectedBytes, cancellationToken);
            _cachedCredentials = credentials;
            _logger.LogInformation("Device credentials saved to protected local storage.");
        }
        catch (Exception exception) when (
            exception is IOException
            or CryptographicException
            or UnauthorizedAccessException
            or PlatformNotSupportedException)
        {
            _logger.LogError(exception, "Failed to save protected device credentials.");
        }
        finally
        {
            _lock.Release();
        }
    }

    private DeviceCredentials? GetConfiguredCredentials()
    {
        var options = _options.CurrentValue;
        return string.IsNullOrWhiteSpace(options.DeviceId) || string.IsNullOrWhiteSpace(options.DeviceToken)
            ? null
            : new DeviceCredentials(options.DeviceId, options.DeviceToken);
    }

    private string GetCredentialPath()
    {
        return Path.Combine(GetCredentialDirectory(), _options.CurrentValue.DeviceCredentialFileName);
    }

    private static string GetCredentialDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "KidGuard",
            "Agent");
    }
}
