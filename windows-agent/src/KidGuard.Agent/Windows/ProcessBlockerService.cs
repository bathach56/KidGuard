using System.Diagnostics;

namespace KidGuard.Agent.Windows;

public sealed class ProcessBlockerService
{
    private readonly ILogger<ProcessBlockerService> _logger;

    public ProcessBlockerService(ILogger<ProcessBlockerService> logger)
    {
        _logger = logger;
    }

    public bool TryBlock(Process process, string processName)
    {
        try
        {
            if (process.HasExited)
            {
                return false;
            }

            process.Kill(entireProcessTree: true);
            return true;
        }
        catch (Exception exception) when (
            exception is InvalidOperationException
            or System.ComponentModel.Win32Exception
            or NotSupportedException)
        {
            _logger.LogWarning(exception, "Failed to block process {ProcessName}.", processName);
            return false;
        }
    }
}
