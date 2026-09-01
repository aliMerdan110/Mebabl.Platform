namespace Mebabl.Platform.Application.Features.Developers.RefreshToken;

public sealed record RefreshDeveloperTokenResponse(
    string AccessToken,
    string RefreshToken);