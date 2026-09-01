using MediatR;

namespace Mebabl.Platform.Application.Features.RolePermissions.RemovePermission;

public sealed record RemovePermissionFromRoleCommand(
    Guid RoleId,
    Guid PermissionId)
    : IRequest;