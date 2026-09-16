using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Content;

public class Post : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid OwnerId { get; set; }

    public string? Text { get; set; }

    public ICollection<PostAttachment> Attachments { get; set; }
        = new List<PostAttachment>();
}