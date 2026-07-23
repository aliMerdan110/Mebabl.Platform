namespace Mebabl.Platform.Domain.Modules.Social.Entities;

public class Mention
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid MentionedUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}