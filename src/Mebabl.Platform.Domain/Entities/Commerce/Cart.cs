using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Commerce;

public class Cart : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public Guid UserId { get; set; }

    public ICollection<CartItem> Items { get; set; }
        = new List<CartItem>();
}