namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatforms;

public sealed record ApplicationPlatformResponse(
    Guid Id,
    Guid ApplicationId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    bool IsActive
);