namespace Mebabl.Platform.Application.Features.Permissions.GetPermissions;

public sealed record PermissionListItem(
    Guid Id,
    string Name,
    string Code,
    string Description,
    string? Category,
    bool IsActive,
    DateTime CreatedAt);