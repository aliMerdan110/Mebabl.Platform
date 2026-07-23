namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Order
{
    public Guid Id { get; set; }

    public Guid ApplicationUserId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}