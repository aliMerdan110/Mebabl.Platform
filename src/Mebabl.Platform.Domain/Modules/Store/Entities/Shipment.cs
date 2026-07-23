namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Shipment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public string Address { get; set; } = string.Empty;

    public string TrackingNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}