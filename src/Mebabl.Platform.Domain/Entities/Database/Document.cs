using System.Text.Json;
using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Database;

public class Document : AuditableEntity
{
    public Guid CollectionId { get; set; }

    public Collection Collection { get; set; } = default!;

    public string Key { get; set; } = Guid.NewGuid().ToString("N");

    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");

    public int Version { get; set; } = 1;

    public string? ETag { get; set; }

    public new bool IsDeleted { get; set; }

    public new DateTime? DeletedAt { get; set; }

    public new Guid? DeletedBy { get; set; }
}