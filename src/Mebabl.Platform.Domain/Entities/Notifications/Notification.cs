using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Notifications;

public class Notification : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid UserId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? Data { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }
}