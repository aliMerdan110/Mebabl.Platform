
using MediatR;
using Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.UpdateProjectPlatform;

public sealed record UpdateProjectPlatformCommand(
    Guid ProjectId,
    Guid PlatformId,
    string Platform,
    string? Nickname,
    string? PackageName,
    string? BundleId,
    string? Domain,
    bool IsActive
) : IRequest<ProjectPlatformDto>;
