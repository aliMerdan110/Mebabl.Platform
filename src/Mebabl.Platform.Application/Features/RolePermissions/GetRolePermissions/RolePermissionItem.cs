namespace Mebabl.Platform.Application.Features.RolePermissions.GetRolePermissions;

public sealed record RolePermissionItem(
    Guid Id,
    string Name,
    string Code,
    string Description,
    string? Category,
    bool IsActive);