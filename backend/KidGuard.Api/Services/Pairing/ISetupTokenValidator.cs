namespace KidGuard.Api.Services.Pairing;

public interface ISetupTokenValidator
{
    bool IsConfigured { get; }
    bool IsValid(string? authorizationHeader);
}
