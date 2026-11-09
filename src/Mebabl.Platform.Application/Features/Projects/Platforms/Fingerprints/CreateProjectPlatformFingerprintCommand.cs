using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;

public sealed record CreateProjectPlatformFingerprintCommand(
    Guid ProjectId,
    Guid PlatformId,
    string FingerprintType,
    string Value)
    : IRequest<ProjectPlatformFingerprintDto>;