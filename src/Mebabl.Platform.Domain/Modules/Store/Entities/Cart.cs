namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Cart
{
    public Guid Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}