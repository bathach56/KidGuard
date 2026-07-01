namespace KidGuard.Api.Entities;

public class PairingRequest
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public Guid DeviceId { get; set; }
    public string ConnectionCode { get; set; } = string.Empty;
    public string Status { get; set; } = PairingRequestStatuses.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Parent { get; set; } = null!;
    public Device Device { get; set; } = null!;
}

public static class PairingRequestStatuses
{
    public const string Pending = "pending";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Expired = "expired";
}
