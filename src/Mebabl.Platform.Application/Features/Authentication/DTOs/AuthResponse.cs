namespace Mebabl.Platform.Application.Features.Authentication.DTOs;

public sealed record AuthResponse(
    Guid ApplicationId,
    string AccessToken,
    string RefreshToken
);