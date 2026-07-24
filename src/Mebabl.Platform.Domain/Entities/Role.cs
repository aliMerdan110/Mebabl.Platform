namespace Mebabl.Platform.Domain.Entities;

public class Role
{
    public Guid Id { get; set; }
    
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}