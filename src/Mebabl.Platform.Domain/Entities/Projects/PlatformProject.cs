
using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Domain.Entities.Projects;

public sealed class PlatformProject : AuditableEntity
{
    public Guid DeveloperId { get; set; }

    public Developer Developer { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<PlatformApplication> Applications { get; set; }
        = new List<PlatformApplication>();
}
