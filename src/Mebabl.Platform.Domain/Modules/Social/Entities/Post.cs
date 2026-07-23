namespace Mebabl.Platform.Domain.Modules.Social.Entities;

public class Post
{
    public Guid Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }
}