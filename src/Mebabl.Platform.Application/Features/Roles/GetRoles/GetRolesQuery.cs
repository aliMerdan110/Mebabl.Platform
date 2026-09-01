using MediatR;

namespace Mebabl.Platform.Application.Features.Roles.GetRoles;

public sealed record GetRolesQuery
    : IRequest<IReadOnlyList<RoleListItem>>;