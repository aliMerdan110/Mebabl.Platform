namespace Mebabl.Platform.Application.Features.Authentication.DTOs;

public sealed record CurrentUserResponse(
    Guid TenantId,
    Guid ApplicationId,
    Guid AccountId,
    Guid UserId,
    string Email
);