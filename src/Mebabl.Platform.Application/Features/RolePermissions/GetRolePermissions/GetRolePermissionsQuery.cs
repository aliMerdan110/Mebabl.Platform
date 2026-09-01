using MediatR;

namespace Mebabl.Platform.Application.Features.RolePermissions.GetRolePermissions;

public sealed record GetRolePermissionsQuery(
    Guid RoleId)
    : IRequest<IReadOnlyList<RolePermissionItem>>;