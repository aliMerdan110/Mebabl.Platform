using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Projects;

public sealed class ProjectSdkConfiguration : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public PlatformProject Project { get; set; } = default!;

    public string ProjectNumber { get; set; } = string.Empty;

    public string PublicKey { get; set; } = string.Empty;

    public string ConfigurationVersion { get; set; } = "1";

    public bool IsActive { get; set; } = true;
}