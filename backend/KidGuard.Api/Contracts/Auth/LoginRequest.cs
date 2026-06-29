using System.ComponentModel.DataAnnotations;

namespace KidGuard.Api.Contracts.Auth;

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password);
