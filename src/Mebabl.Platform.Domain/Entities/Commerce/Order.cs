using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Commerce;

public class Order : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid UserId { get; set; }

    public string Status { get; set; } = "Pending";

    public decimal TotalAmount { get; set; }

    public string Currency { get; set; } = "USD";

    public ICollection<OrderItem> Items { get; set; }
        = new List<OrderItem>();
}