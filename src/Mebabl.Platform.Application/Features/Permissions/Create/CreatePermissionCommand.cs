using MediatR;

namespace Mebabl.Platform.Application.Features.Permissions.Create;

public sealed record CreatePermissionCommand(
    string Name,
    string Code,
    string? Description,
    string? Category)
    : IRequest<CreatePermissionResponse>;