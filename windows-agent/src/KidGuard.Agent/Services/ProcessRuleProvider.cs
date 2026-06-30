using KidGuard.Agent.Models;

namespace KidGuard.Agent.Services;

public sealed class ProcessRuleProvider
{
    private static readonly HashSet<string> StudyBlockedProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "notepad.exe",
        "steam.exe",
        "discord.exe",
        "riotclientservices.exe"
    };

    private static readonly HashSet<string> PunishmentAllowedProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "chrome.exe",
        "browser.exe",
        "coc_coc.exe",
        "msedge.exe",
        "explorer.exe"
    };

    private static readonly HashSet<string> ProtectedSystemProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "System",
        "system.exe",
        "Idle",
        "idle.exe",
        "wininit.exe",
        "winlogon.exe",
        "csrss.exe",
        "smss.exe",
        "services.exe",
        "lsass.exe",
        "explorer.exe",
        "svchost.exe",
        "dwm.exe",
        "fontdrvhost.exe",
        "registry.exe",
        "memory compression.exe",
        "securityhealthservice.exe",
        "kidguard.agent.exe",
        "dotnet.exe"
    };

    public bool ShouldBlock(string processName, AgentMode mode)
    {
        var normalizedProcessName = NormalizeProcessName(processName);

        if (string.IsNullOrWhiteSpace(normalizedProcessName)
            || ProtectedSystemProcesses.Contains(normalizedProcessName))
        {
            return false;
        }

        return mode switch
        {
            AgentMode.Fun => false,
            AgentMode.Study => StudyBlockedProcesses.Contains(normalizedProcessName),
            AgentMode.Punishment => !PunishmentAllowedProcesses.Contains(normalizedProcessName),
            _ => false
        };
    }

    private static string NormalizeProcessName(string processName)
    {
        var normalizedProcessName = processName.Trim();
        return normalizedProcessName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? normalizedProcessName
            : $"{normalizedProcessName}.exe";
    }
}
