using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Domain.Entities.Storage;

public class Bucket : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public PlatformApplication Application { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<StoredFile> Files { get; set; } = new List<StoredFile>();
}