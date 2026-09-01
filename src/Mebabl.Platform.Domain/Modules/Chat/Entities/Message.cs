using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class Message : AuditableEntity
{
    public Guid ConversationId { get; set; }

    public Conversation Conversation { get; set; } = default!;

    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string? MessageType { get; set; }

    public bool IsEdited { get; set; }

    public DateTime? EditedAt { get; set; }

    public ICollection<MessageRead> Reads { get; set; }
    = new List<MessageRead>();
}