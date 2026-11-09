using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Projects;

public sealed class ProjectPlatform : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public PlatformProject Project { get; set; } = default!;

    public string Platform { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public string? PackageName { get; set; }

    public string? BundleId { get; set; }

    public string? Domain { get; set; }

    public bool IsActive { get; set; } = true;
}