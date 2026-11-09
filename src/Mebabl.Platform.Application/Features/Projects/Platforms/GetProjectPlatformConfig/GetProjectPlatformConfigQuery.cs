
using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatformConfig;

public sealed record GetProjectPlatformConfigQuery(
    Guid ProjectId,
    Guid PlatformId,
    Guid ApplicationId
) : IRequest<ProjectPlatformConfigResponse>;
