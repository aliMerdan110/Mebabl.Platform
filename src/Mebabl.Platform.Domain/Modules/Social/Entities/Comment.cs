namespace Mebabl.Platform.Domain.Modules.Social.Entities;

public class Comment
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public Guid ApplicationUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}