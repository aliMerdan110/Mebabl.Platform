using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Storage;

public class StoredFile : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid? BucketId { get; set; }

    public Guid? UserId { get; set; }

    public string Path { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ContentType { get; set; } =
        "application/octet-stream";

    public string Extension { get; set; } = string.Empty;

    public long Size { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string? Hash { get; set; }

    public bool IsPublic { get; set; }

    public string? CacheControl { get; set; }

    public string? ContentDisposition { get; set; }

    public int Version { get; set; } = 1;
}