namespace KidGuard.Api.Entities;

public class Heartbeat
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AgentVersion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Device Device { get; set; } = null!;
}
