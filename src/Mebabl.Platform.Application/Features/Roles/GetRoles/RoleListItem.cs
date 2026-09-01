namespace Mebabl.Platform.Application.Features.Roles.GetRoles;

public sealed record RoleListItem(
    Guid Id,
    string Name,
    string Code,
    string Description,
    bool IsActive,
    DateTime CreatedAt);