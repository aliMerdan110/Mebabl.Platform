namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class MessageReaction
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public Guid ApplicationUserId { get; set; }

    public string Reaction { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}