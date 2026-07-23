namespace Mebabl.Platform.Domain.Modules.Chat.Entities;

public class MessageAttachment
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}