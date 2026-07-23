namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Review
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public Guid ApplicationUserId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}