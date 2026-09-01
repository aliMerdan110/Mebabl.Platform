namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatformConfig;

public sealed record ApplicationPlatformConfigResponse(
    Guid ApplicationId,
    Guid PlatformId,
    string Platform,
    string? PackageName,
    string? BundleId,
    string? Domain,
    string ApiKey,
    string BaseUrl
);