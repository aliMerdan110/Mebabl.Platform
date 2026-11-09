namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatformConfig;

public sealed record ProjectPlatformConfigResponse(
    string ConfigurationVersion,
    Guid ProjectId,
    string ProjectNumber,
    Guid ApplicationId,
    string ApplicationName,
    string ApplicationCode,
    Guid PlatformId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    string PublicKey,
    string ApiKey,
    string BaseUrl
);