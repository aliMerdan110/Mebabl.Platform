using MediatR;

namespace Mebabl.Platform.Application.Features.Roles.Create;

public sealed record CreateRoleCommand(
    string Name,
    string Code,
    string? Description)
    : IRequest<CreateRoleResponse>;