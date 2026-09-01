namespace Mebabl.Platform.Application.Features.Users.Login;

public sealed record LoginUserResponse(
    Guid AccountId,
    Guid UserId,
    string AccessToken,
    string RefreshToken);