using MediatR;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;

public sealed record DeleteProjectPlatformFingerprintCommand(
    Guid ProjectId,
    Guid PlatformId,
    Guid FingerprintId)
    : IRequest;