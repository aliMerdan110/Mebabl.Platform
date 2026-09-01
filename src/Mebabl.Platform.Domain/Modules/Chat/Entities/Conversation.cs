using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class Conversation : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public string? Title { get; set; }

    public bool IsGroup { get; set; }

    public ICollection<ConversationParticipant> Participants { get; set; }
        = new List<ConversationParticipant>();

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();
}