using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Social;

public class SocialRepost : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid PostId { get; set; }

    public Guid UserId { get; set; }
}