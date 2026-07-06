using System.Diagnostics;

namespace KidGuard.Client.Services;

public sealed class AgentServiceManager
{
    private const string ServiceName = "KidGuardAgent";

    public async Task<AgentServiceStatus> GetStatusAsync(CancellationToken cancellationToken)
    {
        var queryConfig = await RunScAsync($"qc {ServiceName}", cancellationToken);
        if (queryConfig.ExitCode != 0)
        {
            return new AgentServiceStatus(
                IsInstalled: false,
                IsRunning: false,
                RunsAsLocalSystem: false,
                Message: "KidGuardAgent service is not installed. Install it from an Administrator PowerShell before testing protection.");
        }

        var runsAsLocalSystem = queryConfig.Output.Contains("LocalSystem", StringComparison.OrdinalIgnoreCase);
        var queryStatus = await RunScAsync($"query {ServiceName}", cancellationToken);
        var isRunning = queryStatus.Output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);

        if (!runsAsLocalSystem)
        {
            return new AgentServiceStatus(
                IsInstalled: true,
                IsRunning: isRunning,
                RunsAsLocalSystem: false,
                Message: "KidGuardAgent is installed but is not running as LocalSystem. Reinstall the service as Administrator.");
        }

        if (!isRunning)
        {
            var startResult = await RunScAsync($"start {ServiceName}", cancellationToken);
            queryStatus = await RunScAsync($"query {ServiceName}", cancellationToken);
            isRunning = queryStatus.Output.Contains("RUNNING", StringComparison.OrdinalIgnoreCase);

            if (!isRunning)
            {
                return new AgentServiceStatus(
                    IsInstalled: true,
                    IsRunning: false,
                    RunsAsLocalSystem: true,
                    Message: $"KidGuardAgent is installed as LocalSystem but could not be started automatically. Start it from Administrator PowerShell. {startResult.Output}".Trim());
            }
        }

        return new AgentServiceStatus(
            IsInstalled: true,
            IsRunning: true,
            RunsAsLocalSystem: true,
            Message: "KidGuardAgent service is running as LocalSystem. Protection can use administrator-level service permissions.");
    }

    private static async Task<CommandResult> RunScAsync(string arguments, CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "sc.exe",
            Arguments = arguments,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start sc.exe.");

        var standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return new CommandResult(
            process.ExitCode,
            $"{standardOutput}{Environment.NewLine}{standardError}".Trim());
    }

    private sealed record CommandResult(int ExitCode, string Output);
}
