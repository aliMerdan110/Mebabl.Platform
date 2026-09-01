namespace Mebabl.Platform.Application.Features.Applications.GetApplicationById;

public sealed record GetApplicationByIdResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    string? Domain,
    bool IsActive,
    DateTime CreatedAt);