namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class Product
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}