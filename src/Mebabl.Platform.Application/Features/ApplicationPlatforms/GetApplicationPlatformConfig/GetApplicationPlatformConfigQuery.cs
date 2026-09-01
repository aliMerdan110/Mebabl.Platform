using MediatR;

namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatformConfig;

public sealed record GetApplicationPlatformConfigQuery(
    Guid ApplicationId,
    Guid PlatformId
) : IRequest<ApplicationPlatformConfigResponse>;