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
        "explorer.exe"
    };

    private static readonly HashSet<string> ProtectedSystemProcesses = new(StringComparer.OrdinalIgnoreCase)
    {
        "System",
        "Idle",
        "wininit.exe",
        "winlogon.exe",
        "csrss.exe",
        "smss.exe",
        "services.exe",
        "lsass.exe",
        "explorer.exe",
        "svchost.exe",
        "dwm.exe"
    };

    public bool ShouldBlock(string processName, AgentMode mode)
    {
        if (string.IsNullOrWhiteSpace(processName) || ProtectedSystemProcesses.Contains(processName))
        {
            return false;
        }

        return mode switch
        {
            AgentMode.Fun => false,
            AgentMode.Study => StudyBlockedProcesses.Contains(processName),
            AgentMode.Punishment => !PunishmentAllowedProcesses.Contains(processName),
            _ => false
        };
    }
}
