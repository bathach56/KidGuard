namespace KidGuard.Api.Contracts.Pairing;

public record ChildPairingDecisionRequest(
    string ConnectionCode,
    Guid PairingRequestId);
