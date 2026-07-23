namespace Mebabl.Platform.Domain.Modules.Social.Entities;

public class Media
{
    public Guid Id { get; set; }

    public Guid PostId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Order { get; set; }

    public DateTime CreatedAt { get; set; }
}