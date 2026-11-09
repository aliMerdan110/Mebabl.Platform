using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Domain.Entities.Database;

/// <summary>
/// A collection belongs to a project, so all applications registered
/// under that project can access the same database resource subject
/// to the project's database security rules.
/// </summary>
public sealed class Collection : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public PlatformProject Project { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsSystem { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
