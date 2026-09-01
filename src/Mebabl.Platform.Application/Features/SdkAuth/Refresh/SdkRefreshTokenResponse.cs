namespace Mebabl.Platform.Application.Features.SdkAuth.Refresh;

public sealed record SdkRefreshTokenResponse(
    Guid AccountId,
    Guid UserId,
    string AccessToken,
    string RefreshToken);