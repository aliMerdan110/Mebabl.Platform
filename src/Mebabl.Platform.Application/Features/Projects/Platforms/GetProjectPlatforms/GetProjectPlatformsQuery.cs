
using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;

public sealed record GetProjectPlatformsQuery(
    Guid ProjectId
) : IRequest<IReadOnlyList<ProjectPlatformDto>>;
