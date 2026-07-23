namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class BlockedUser
{
    public Guid Id { get; set; }

    public Guid BlockerId { get; set; }

    public Guid BlockedId { get; set; }

    public DateTime CreatedAt { get; set; }
}