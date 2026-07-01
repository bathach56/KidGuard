namespace KidGuard.Api.Contracts.Auth;

public record RegisterResponse(
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber);
