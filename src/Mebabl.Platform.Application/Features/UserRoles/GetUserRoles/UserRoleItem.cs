namespace Mebabl.Platform.Application.Features.UserRoles.GetUserRoles;

public sealed record UserRoleItem(
    Guid Id,
    string Name,
    string Code,
    string Description,
    bool IsActive);