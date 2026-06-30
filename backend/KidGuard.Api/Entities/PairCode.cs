namespace KidGuard.Api.Entities;

public class PairCode
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public Device Device { get; set; } = null!;
}
