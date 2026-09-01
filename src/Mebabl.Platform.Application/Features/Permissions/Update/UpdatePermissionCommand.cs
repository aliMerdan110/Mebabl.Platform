using MediatR;

namespace Mebabl.Platform.Application.Features.Permissions.Update;

public sealed record UpdatePermissionCommand(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string? Category,
    bool IsActive)
    : IRequest;