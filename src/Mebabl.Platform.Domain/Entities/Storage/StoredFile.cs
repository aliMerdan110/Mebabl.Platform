using System.Text.Json;
using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Storage;

public class StoredFile : AuditableEntity
{
    public Guid BucketId { get; set; }

    public Bucket Bucket { get; set; } = default!;

    public string Key { get; set; } = Guid.NewGuid().ToString("N");

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public long Size { get; set; }

    public string Hash { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public JsonDocument Metadata { get; set; } = JsonDocument.Parse("{}");

    public int Version { get; set; } = 1;
}