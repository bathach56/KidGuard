namespace KidGuard.Agent.Models;

public sealed class ActivityLogEntry
{
    public string ProcessName { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public AgentMode Mode { get; init; } = AgentMode.Fun;

    public string Message { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
