using MediatR;

namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.GetApplicationPlatforms;

public sealed record GetApplicationPlatformsQuery(
    Guid ApplicationId
) : IRequest<IReadOnlyList<ApplicationPlatformResponse>>;