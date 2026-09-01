using MediatR;

namespace Mebabl.Platform.Application.Features.Roles.Update;

public sealed record UpdateRoleCommand(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive)
    : IRequest;