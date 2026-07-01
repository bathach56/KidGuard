using System.ComponentModel.DataAnnotations;

namespace KidGuard.Api.Contracts.Auth;

public record RegisterRequest(
    [property: Required, MaxLength(100)] string FullName,
    [property: Required, EmailAddress, MaxLength(255)] string Email,
    [property: Required, MinLength(8)] string Password,
    [property: MaxLength(20)] string? PhoneNumber);
