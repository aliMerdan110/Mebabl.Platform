namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Wishlist
{
    public Guid Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}