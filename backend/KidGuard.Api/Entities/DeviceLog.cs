namespace KidGuard.Api.Entities;

public class DeviceLog
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Device Device { get; set; } = null!;
}
