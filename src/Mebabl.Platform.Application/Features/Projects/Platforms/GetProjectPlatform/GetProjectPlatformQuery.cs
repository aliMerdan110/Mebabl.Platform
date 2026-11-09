using MediatR;
using Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatform;

public sealed record GetProjectPlatformQuery(
    Guid ProjectId,
    Guid PlatformId
) : IRequest<ProjectPlatformDto>;