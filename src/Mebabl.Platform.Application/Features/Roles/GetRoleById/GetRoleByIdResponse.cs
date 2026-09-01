namespace Mebabl.Platform.Application.Features.Roles.GetRoleById;

public sealed record GetRoleByIdResponse(
    Guid Id,
    string Name,
    string Code,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);