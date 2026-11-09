namespace Mebabl.Platform.Application.Features.Applications.GetApplicationById;

public sealed record GetApplicationByIdResponse(
    Guid Id,
    Guid ProjectId,
    Guid ProjectPlatformId,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain);