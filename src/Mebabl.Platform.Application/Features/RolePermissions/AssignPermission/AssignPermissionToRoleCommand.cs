using MediatR;

namespace Mebabl.Platform.Application.Features.RolePermissions.AssignPermission;

public sealed record AssignPermissionToRoleCommand(
    Guid RoleId,
    Guid PermissionId)
    : IRequest;