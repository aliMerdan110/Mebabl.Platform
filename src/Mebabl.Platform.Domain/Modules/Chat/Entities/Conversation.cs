namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class Conversation
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public DateTime CreatedAt { get; set; }
}