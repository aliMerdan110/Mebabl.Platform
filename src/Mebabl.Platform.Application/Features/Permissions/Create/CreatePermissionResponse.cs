namespace Mebabl.Platform.Application.Features.Permissions.Create;

public sealed record CreatePermissionResponse(
    Guid Id,
    string Name,
    string Code);