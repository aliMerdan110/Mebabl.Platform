
namespace Mebabl.Platform.Application.Features.Projects.Platforms.CreateProjectPlatform;

public sealed record CreateProjectPlatformResponse(
    Guid Id,
    Guid ProjectId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    bool IsActive
);
