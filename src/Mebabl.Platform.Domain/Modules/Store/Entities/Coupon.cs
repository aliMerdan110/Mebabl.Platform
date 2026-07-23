namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Coupon
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public string Code { get; set; } = string.Empty;

    public decimal DiscountAmount { get; set; }

    public bool IsPercentage { get; set; }

    public DateTime ExpiryDate { get; set; }

    public DateTime CreatedAt { get; set; }
}