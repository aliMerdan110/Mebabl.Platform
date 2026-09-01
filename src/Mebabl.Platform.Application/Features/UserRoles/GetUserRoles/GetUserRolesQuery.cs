using MediatR;

namespace Mebabl.Platform.Application.Features.UserRoles.GetUserRoles;

public sealed record GetUserRolesQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<UserRoleItem>>;