namespace Mebabl.Platform.Application.Features.Users.Register;

public sealed record RegisterUserResponse(
    Guid AccountId,
    Guid UserId,
    string AccessToken,
    string RefreshToken);