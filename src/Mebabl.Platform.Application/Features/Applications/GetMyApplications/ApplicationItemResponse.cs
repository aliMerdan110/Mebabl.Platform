namespace Mebabl.Platform.Application.Features.Applications.GetMyApplications;

public sealed record ApplicationItemResponse(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    bool IsActive,
    Guid ProjectPlatformId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    DateTime CreatedAt,
    DateTime? UpdatedAt);