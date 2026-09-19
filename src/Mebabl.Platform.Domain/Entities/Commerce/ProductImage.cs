using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Commerce;

public class ProductImage : AuditableEntity
{
    public Guid ProductId { get; set; }

    public Guid StorageFileId { get; set; }

    public int Order { get; set; }

    public Product Product { get; set; } = null!;
}