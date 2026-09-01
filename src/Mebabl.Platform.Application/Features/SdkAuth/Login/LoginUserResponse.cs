namespace Mebabl.Platform.Application.Features.SdkAuth.Login;

public sealed record LoginUserResponse(
    Guid AccountId,
    Guid UserId,
    string AccessToken,
    string RefreshToken);