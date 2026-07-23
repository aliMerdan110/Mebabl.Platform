namespace Mebabl.Platform.Domain.Modules.Social.Entities;

public class Share
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid ApplicationUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}