using MediatR;

namespace Mebabl.Platform.Application.Features.UserRoles.RemoveRole;

public sealed record RemoveRoleFromUserCommand(
    Guid UserId,
    Guid RoleId)
    : IRequest;