using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Domain.Entities.Content;

public class PostAttachment : AuditableEntity
{
    public Guid PostId { get; set; }

    public Guid StorageFileId { get; set; }

    public string Type { get; set; } = "File";

    public int Order { get; set; }

    public Post Post { get; set; } = null!;

    public StoredFile StorageFile { get; set; } = null!;
}