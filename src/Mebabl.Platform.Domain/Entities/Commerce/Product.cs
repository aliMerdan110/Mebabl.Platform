using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Commerce;

public class Product : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid OwnerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public int StockQuantity { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ProductImage> Images { get; set; }
        = new List<ProductImage>();

    public ICollection<CartItem> CartItems { get; set; }
        = new List<CartItem>();

    public ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();
}