using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;

public sealed record GetProjectPlatformFingerprintsQuery(
    Guid ProjectId,
    Guid PlatformId)
    : IRequest<IReadOnlyList<ProjectPlatformFingerprintDto>>;