using Microsoft.Extensions.Options;

namespace KidGuard.Agent.Configuration;

public sealed class AgentOptionsValidator : IValidateOptions<AgentOptions>
{
    public ValidateOptionsResult Validate(string? name, AgentOptions options)
    {
        var failures = new List<string>();

        if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out _))
        {
            failures.Add("Agent:ApiBaseUrl must be an absolute URL.");
        }

        if (string.IsNullOrWhiteSpace(options.DeviceName))
        {
            failures.Add("Agent:DeviceName is required.");
        }

        if (string.IsNullOrWhiteSpace(options.AgentVersion))
        {
            failures.Add("Agent:AgentVersion is required.");
        }

        if (options.HeartbeatIntervalSeconds <= 0)
        {
            failures.Add("Agent:HeartbeatIntervalSeconds must be greater than 0.");
        }

        if (options.ModeSyncIntervalSeconds <= 0)
        {
            failures.Add("Agent:ModeSyncIntervalSeconds must be greater than 0.");
        }

        if (options.ProcessScanIntervalSeconds <= 0)
        {
            failures.Add("Agent:ProcessScanIntervalSeconds must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(options.LocalCacheFileName))
        {
            failures.Add("Agent:LocalCacheFileName is required.");
        }

        if (string.IsNullOrWhiteSpace(options.DeviceCredentialFileName))
        {
            failures.Add("Agent:DeviceCredentialFileName is required.");
        }

        var hasDeviceToken = !string.IsNullOrWhiteSpace(options.DeviceToken);
        var hasDeviceId = !string.IsNullOrWhiteSpace(options.DeviceId);

        if (hasDeviceToken && !hasDeviceId)
        {
            failures.Add("Agent:DeviceId is required when Agent:DeviceToken is configured.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
