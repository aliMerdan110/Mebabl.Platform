namespace Mebabl.Platform.Application.Features.Applications.Credentials.GetCredentials;

public sealed record ApplicationCredentialResponse(
    Guid Id,
    string ApiKey,
    bool IsActive,
    DateTime CreatedAt);