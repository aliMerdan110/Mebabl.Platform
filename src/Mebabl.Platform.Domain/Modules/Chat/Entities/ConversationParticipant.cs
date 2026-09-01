using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class ConversationParticipant : AuditableEntity
{
    public Guid ConversationId { get; set; }

    public Conversation Conversation { get; set; } = default!;

    public Guid UserId { get; set; }

    public DateTime? JoinedAt { get; set; }

    public DateTime? LeftAt { get; set; }

    public DateTime? LastReadAt { get; set; }

    public bool IsAdmin { get; set; }
}