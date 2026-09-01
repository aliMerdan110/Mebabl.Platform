using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class MessageReaction : AuditableEntity
{
    public Guid MessageId { get; set; }

    public Message Message { get; set; } = default!;

    public Guid UserId { get; set; }

    public string Reaction { get; set; } = string.Empty;
}