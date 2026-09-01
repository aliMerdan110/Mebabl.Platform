namespace Mebabl.Platform.Application.Features.Permissions.GetPermissionById;

public sealed record GetPermissionByIdResponse(
    Guid Id,
    string Name,
    string Code,
    string Description,
    string? Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);