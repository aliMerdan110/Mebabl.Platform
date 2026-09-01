using MediatR;

namespace Mebabl.Platform.Application.Features.Roles.Delete;

public sealed record DeleteRoleCommand(Guid Id)
    : IRequest;