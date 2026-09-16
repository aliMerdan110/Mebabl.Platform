using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class Account : AuditableEntity
{
    public Profile? Profile { get; set; }

    public ICollection<ApplicationUser> ApplicationUsers { get; set; }
        = new List<ApplicationUser>();
}