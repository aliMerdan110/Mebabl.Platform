namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class ConversationParticipant
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid ApplicationUserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsActive { get; set; }
}