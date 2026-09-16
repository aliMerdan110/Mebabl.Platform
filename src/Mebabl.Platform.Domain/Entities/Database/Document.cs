using System.Text.Json;

namespace Mebabl.Platform.Domain.Entities.Database;

public sealed class Document
{
    public Guid Id { get; set; }

    public Guid CollectionId { get; set; }

    public Guid ApplicationId { get; set; }

    public Guid? UserId { get; set; }

    public string Key { get; set; } = null!;

    public JsonDocument Data { get; set; } = JsonDocument.Parse("{}");

    public int Version { get; set; } = 1;

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Collection Collection { get; set; } = null!;
}