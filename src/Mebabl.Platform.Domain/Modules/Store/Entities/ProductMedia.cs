namespace Mebabl.Platform.Domain.Modules.Store.Entities;

public class ProductMedia
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Order { get; set; }

    public DateTime CreatedAt { get; set; }
}