namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.DTOs;

public sealed record ApplicationAuthResponse(
    Guid ApplicationId,
    string AccessToken);