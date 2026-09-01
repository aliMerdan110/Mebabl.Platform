using MediatR;

namespace Mebabl.Platform.Application.Features.Permissions.Delete;

public sealed record DeletePermissionCommand(Guid Id)
    : IRequest;