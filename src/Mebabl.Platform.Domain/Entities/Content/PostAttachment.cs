using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Domain.Entities.Content;

public class PostAttachment : AuditableEntity
{
    public Guid PostId { get; set; }

    public Post Post { get; set; } = null!;

    public Guid StorageFileId { get; set; }

    public StoredFile StorageFile { get; set; } = null!;

    public string Type { get; set; } = "File";

    public int Order { get; set; }
}