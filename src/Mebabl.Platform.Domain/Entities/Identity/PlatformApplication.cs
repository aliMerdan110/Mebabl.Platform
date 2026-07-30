using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class PlatformApplication : AuditableEntity
{
   

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Domain { get; set; }

    public bool IsActive { get; set; } = true;


    

    public ICollection<ApplicationUser> Users { get; set; }
        = new List<ApplicationUser>();

    // تمت إضافة هذه العلاقات لسهولة الوصول لأدوار وصلاحيات التطبيق
    public ICollection<Role> Roles { get; set; } 
        = new List<Role>();

    public ICollection<Permission> Permissions { get; set; } 
        = new List<Permission>();
}