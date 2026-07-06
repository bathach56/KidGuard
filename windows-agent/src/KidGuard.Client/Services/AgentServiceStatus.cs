namespace KidGuard.Client.Services;

public sealed record AgentServiceStatus(
    bool IsInstalled,
    bool IsRunning,
    bool RunsAsLocalSystem,
    string Message);
