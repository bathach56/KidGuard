using KidGuard.Agent.Infrastructure;
using KidGuard.Agent.Models;

namespace KidGuard.Agent.Services;

public sealed class PairingService
{
    private readonly BackendApiClient _backendApiClient;
    private readonly DeviceCredentialStore _deviceCredentialStore;
    private readonly ILogger<PairingService> _logger;
    private DateTimeOffset _nextPairCodeRequestAt = DateTimeOffset.MinValue;

    public PairingService(
        BackendApiClient backendApiClient,
        DeviceCredentialStore deviceCredentialStore,
        ILogger<PairingService> logger)
    {
        _backendApiClient = backendApiClient;
        _deviceCredentialStore = deviceCredentialStore;
        _logger = logger;
    }

    public async Task<bool> IsPairedAsync(CancellationToken cancellationToken)
    {
        var credentials = await _deviceCredentialStore.GetCredentialsAsync(cancellationToken);
        return credentials is not null;
    }

    public async Task EnsurePairCodeAsync(CancellationToken cancellationToken)
    {
        if (await IsPairedAsync(cancellationToken) || DateTimeOffset.UtcNow < _nextPairCodeRequestAt)
        {
            return;
        }

        var pairCode = await _backendApiClient.CreatePairCodeAsync(cancellationToken);
        if (pairCode is null)
        {
            _nextPairCodeRequestAt = DateTimeOffset.UtcNow.AddMinutes(1);
            _logger.LogWarning("Device is not paired. Failed to create pair code. The agent will retry in 1 minute.");
            return;
        }

        _nextPairCodeRequestAt = DateTimeOffset.UtcNow.AddSeconds(pairCode.ExpiresIn);
        _logger.LogInformation(
            "Device is not paired. Pair code: {PairCode}. Expires in {ExpiresIn} seconds.",
            pairCode.PairCode,
            pairCode.ExpiresIn);
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
