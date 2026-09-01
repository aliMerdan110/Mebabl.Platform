using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Applications;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class PlatformApplication : AuditableEntity
{

    public ICollection<ApplicationAuthProvider> AuthProviders { get; set; }
    = new List<ApplicationAuthProvider>();

    public Guid DeveloperId { get; set; }

    public Developer Developer { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Domain { get; set; }

    public bool IsActive { get; set; } = true;


// 
    public ICollection<ApplicationCredential> Credentials { get; set; }
        = new List<ApplicationCredential>();

    public ICollection<ApplicationUser> Users { get; set; }
        = new List<ApplicationUser>();

    public ICollection<Role> Roles { get; set; }
        = new List<Role>();

    public ICollection<Permission> Permissions { get; set; }
        = new List<Permission>();

        //  اضافه تطبيق جديد 
    public ICollection<ApplicationPlatform> Platforms { get; set; }
    = new List<ApplicationPlatform>();
}