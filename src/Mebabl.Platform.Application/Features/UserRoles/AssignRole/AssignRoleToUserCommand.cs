using MediatR;

namespace Mebabl.Platform.Application.Features.UserRoles.AssignRole;

public sealed record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId)
    : IRequest;