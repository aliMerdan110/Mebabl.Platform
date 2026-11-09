using System.Text.Json;

using Mebabl.Platform.Domain.Entities.Identity;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Domain.Entities.Database;

/// <summary>
/// A document is owned by a project-scoped collection.
/// ProjectId is stored explicitly as a tenant boundary and must match
/// the ProjectId of the referenced collection.
/// Creator identifiers are audit metadata, not authorization decisions.
/// </summary>
public sealed class Document
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public PlatformProject Project { get; set; } = null!;

    public Guid CollectionId { get; set; }

    public Collection Collection { get; set; } = null!;

    /// <summary>
    /// Application that created the document. This is provenance metadata;
    /// it must not be used to partition project-shared document reads.
    /// </summary>
    public Guid CreatedByApplicationId { get; set; }

    /// <summary>
    /// User that created the document, when the operation was user initiated.
    /// Authorization must be evaluated by the database security service.
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    public string Key { get; set; } = string.Empty;

    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// Monotonically increasing document version used for optimistic concurrency.
    /// </summary>
    public int Version { get; set; } = 1;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
