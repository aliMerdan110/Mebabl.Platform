namespace Mebabl.Platform.Domain.Modules.Social.Entities;

public class Notification
{
    public Guid Id { get; set; }

    public Guid ReceiverId { get; set; }

    public Guid? SenderId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}