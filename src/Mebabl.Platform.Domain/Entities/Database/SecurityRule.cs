using System.Text.Json;

using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Domain.Entities.Database;

/// <summary>
/// Stores the versioned security policy for a project-owned collection.
/// ProjectId is an explicit isolation boundary and must match the project
/// that owns the referenced collection.
/// </summary>
public sealed class SecurityRule : AuditableEntity
{
    /// <summary>
    /// Project that owns this policy. This must match Collection.ProjectId.
    /// </summary>
    public Guid ProjectId { get; set; }

    public PlatformProject Project { get; set; } = null!;

    public Guid CollectionId { get; set; }

    public Collection Collection { get; set; } = null!;

    /// <summary>
    /// Versioned declarative policy document. This is data only; it must never
    /// be compiled or executed as C# or arbitrary .NET code.
    /// An empty statement list is intentionally fail-closed.
    /// </summary>
    public JsonDocument Definition { get; set; } = JsonDocument.Parse(
        """{"version":1,"statements":[]}""");

    /// <summary>
    /// Monotonically increasing policy revision used for optimistic concurrency.
    /// </summary>
    public int Revision { get; set; } = 1;

    /// <summary>
    /// Indicates whether this policy is enabled. Evaluation must still reject
    /// policies that have not been successfully published.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Set only after the policy has passed validation and has been published.
    /// A null value means the policy must not authorize any operation.
    /// </summary>
    public DateTime? PublishedAt { get; set; }
}
