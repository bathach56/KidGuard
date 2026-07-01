namespace KidGuard.Api.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string? DeviceToken { get; set; }
    public string CurrentMode { get; set; } = "fun";
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }
    public Mode? Mode { get; set; }
    public ICollection<DeviceLog> DeviceLogs { get; set; } = new List<DeviceLog>();
    public ICollection<Heartbeat> Heartbeats { get; set; } = new List<Heartbeat>();
    public PairCode? PairCode { get; set; }
    public ICollection<PairingRequest> PairingRequests { get; set; } = new List<PairingRequest>();
}
