using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Projects;

public sealed class ProjectPlatformFingerprint : AuditableEntity
{
    public Guid ProjectPlatformId { get; set; }

    public ProjectPlatform ProjectPlatform { get; set; } = default!;

    public string FingerprintType { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}