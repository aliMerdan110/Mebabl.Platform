
namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;

public sealed record ProjectPlatformDto(
    Guid Id,
    Guid ProjectId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    bool IsActive);
