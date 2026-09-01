using MediatR;

namespace Mebabl.Platform.Application.Features.Permissions.GetPermissions;

public sealed record GetPermissionsQuery
    : IRequest<IReadOnlyList<PermissionListItem>>;