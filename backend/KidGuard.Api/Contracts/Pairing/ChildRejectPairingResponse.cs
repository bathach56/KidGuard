namespace KidGuard.Api.Contracts.Pairing;

public record ChildRejectPairingResponse(
    Guid PairingRequestId,
    string Status,
    DateTime RejectedAt);
