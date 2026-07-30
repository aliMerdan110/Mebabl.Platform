using Mebabl.Platform.Domain.Common.Entities;
namespace Mebabl.Platform.Domain.Entities.Identity;

public class ApplicationUserRole  : AuditableEntity
{

    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = default!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;

}