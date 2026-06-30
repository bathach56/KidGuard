using KidGuard.Agent.Models;

namespace KidGuard.Agent.Services;

public sealed class AgentCommandRunner
{
    private const string SaveCredentialsCommand = "--save-credentials";

    private readonly DeviceCredentialStore _deviceCredentialStore;
    private readonly ILogger<AgentCommandRunner> _logger;

    public AgentCommandRunner(
        DeviceCredentialStore deviceCredentialStore,
        ILogger<AgentCommandRunner> logger)
    {
        _deviceCredentialStore = deviceCredentialStore;
        _logger = logger;
    }

    public static bool IsCommandMode(string[] args)
    {
        return args.Length > 0 && string.Equals(args[0], SaveCredentialsCommand, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken)
    {
        if (!IsCommandMode(args))
        {
            return 0;
        }

        if (args.Length != 3)
        {
            _logger.LogError("Usage: KidGuard.Agent --save-credentials <deviceId> <deviceToken>");
            return 1;
        }

        var deviceId = args[1];
        var deviceToken = args[2];

        if (!Guid.TryParse(deviceId, out _) || string.IsNullOrWhiteSpace(deviceToken))
        {
            _logger.LogError("DeviceId must be a valid GUID and DeviceToken is required.");
            return 1;
        }

        await _deviceCredentialStore.SaveCredentialsAsync(
            new DeviceCredentials(deviceId, deviceToken),
            cancellationToken);

        _logger.LogInformation("Device credentials were saved successfully.");
        return 0;
    }
}
