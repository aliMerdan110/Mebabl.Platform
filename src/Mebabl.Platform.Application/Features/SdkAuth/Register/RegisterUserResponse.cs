namespace Mebabl.Platform.Application.Features.SdkAuth.Register;

public sealed record RegisterUserResponse(
    Guid AccountId,
    Guid UserId,
    string AccessToken,
    string RefreshToken);