namespace Mebabl.Platform.Application.Features.Applications.Users.CreateApplicationUser;

public sealed record CreateApplicationUserResponse(
    Guid Id,
    Guid ApplicationId,
    Guid AccountId,
    string Email,
    string Username,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAt
);