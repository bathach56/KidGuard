using KidGuard.Agent.Infrastructure;
using KidGuard.Agent.Models;

namespace KidGuard.Agent.Services;

public sealed class PairingService
{
    private readonly DeviceCredentialStore _deviceCredentialStore;
    private readonly ILogger<PairingService> _logger;
    private bool _hasLoggedWaitingForApproval;

    public PairingService(
        DeviceCredentialStore deviceCredentialStore,
        ILogger<PairingService> logger)
    {
        _deviceCredentialStore = deviceCredentialStore;
        _logger = logger;
    }

    public async Task<bool> IsPairedAsync(CancellationToken cancellationToken)
    {
        var credentials = await _deviceCredentialStore.GetCredentialsAsync(cancellationToken);
        var isPaired = credentials is not null;
        if (isPaired)
        {
            _hasLoggedWaitingForApproval = false;
        }

        return isPaired;
    }

    public void LogWaitingForApproval()
    {
        if (_hasLoggedWaitingForApproval)
        {
            return;
        }

        _hasLoggedWaitingForApproval = true;
        _logger.LogInformation("Device is not approved yet. Waiting for Windows Client to save approved credentials.");
    }

    public Task SaveDeviceCredentialsAsync(
        string deviceId,
        string deviceToken,
        CancellationToken cancellationToken)
    {
        return _deviceCredentialStore.SaveCredentialsAsync(
            new DeviceCredentials(deviceId, deviceToken),
            cancellationToken);
    }
}
