namespace Mebabl.Platform.Application.Features.Roles.Create;

public sealed record CreateRoleResponse(
    Guid Id,
    string Name,
    string Code);