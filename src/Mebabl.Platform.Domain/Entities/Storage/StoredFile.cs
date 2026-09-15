
using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Domain.Entities.Storage;

public sealed class StoredFile : AuditableEntity
{
public Guid ApplicationId { get; set; }

public Guid? UserId { get; set; }

public string Name { get; set; } = null!;

public string Path { get; set; } = null!;

public string FileName { get; set; } = null!;

public string ContentType { get; set; } = null!;

public string Extension { get; set; } = null!;

public long Size { get; set; }

public string? Hash { get; set; }

public int Version { get; set; }

public string StorageKey { get; set; } = null!;

public string? CacheControl { get; set; }

public string? ContentDisposition { get; set; }

public bool IsPublic { get; set; }

public bool IsDeleted { get; set; }

public DateTime? DeletedAt { get; set; }

public PlatformApplication Application { get; set; } = null!;


}
