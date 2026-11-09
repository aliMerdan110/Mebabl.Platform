namespace Mebabl.Platform.Application.Features.Projects.Platforms.Fingerprints;

public sealed record ProjectPlatformFingerprintDto(
    Guid Id,
    Guid ProjectPlatformId,
    string FingerprintType,
    string Value,
    DateTime CreatedAt);