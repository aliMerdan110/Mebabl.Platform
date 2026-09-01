using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Storage;

namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class MessageAttachment : AuditableEntity
{
    public Guid MessageId { get; set; }

    public Message Message { get; set; } = default!;

    public Guid StoredFileId { get; set; }

    public StoredFile StoredFile { get; set; } = default!;

    public string? Caption { get; set; }
}