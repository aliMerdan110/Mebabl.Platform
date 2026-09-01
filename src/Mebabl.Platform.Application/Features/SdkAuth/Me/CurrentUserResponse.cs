namespace Mebabl.Platform.Application.Features.SdkAuth.Me;

public sealed record CurrentUserResponse(
    Guid AccountId,
    Guid UserId,
    Guid ApplicationId,
    string Email,
    string Username,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);