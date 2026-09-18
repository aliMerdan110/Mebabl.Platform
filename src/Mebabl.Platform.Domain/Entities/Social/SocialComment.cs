using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Social;

public class SocialComment : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid PostId { get; set; }

    public Guid OwnerId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string Text { get; set; } = string.Empty;

    public SocialComment? ParentComment { get; set; }

    public ICollection<SocialComment> Replies { get; set; }
        = new List<SocialComment>();
}