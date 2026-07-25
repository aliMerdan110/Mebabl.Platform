namespace Mebabl.Platform.Application.Features.Authentication.DTOs;

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken
);