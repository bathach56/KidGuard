namespace KidGuard.Agent.Models;

public sealed class AgentCache
{
    public AgentMode CurrentMode { get; set; } = AgentMode.Fun;

    public DateTimeOffset? LastSuccessfulSyncAt { get; set; }

    public List<ActivityLogEntry> PendingLogs { get; set; } = [];
}
