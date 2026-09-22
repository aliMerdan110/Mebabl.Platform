
using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Applications;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class PlatformApplication : AuditableEntity
{
    public Guid ProjectId { get; set; }

    public PlatformProject Project { get; set; } = default!;

    public Guid DeveloperId { get; set; }

    public Developer Developer { get; set; } = default!;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Domain { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ApplicationAuthProvider> AuthProviders { get; set; }
        = new List<ApplicationAuthProvider>();

    public ICollection<ApplicationCredential> Credentials { get; set; }
        = new List<ApplicationCredential>();

    public ICollection<ApplicationUser> Users { get; set; }
        = new List<ApplicationUser>();

    public ICollection<Role> Roles { get; set; }
        = new List<Role>();

    public ICollection<Permission> Permissions { get; set; }
        = new List<Permission>();

    public ICollection<ApplicationPlatform> Platforms { get; set; }
        = new List<ApplicationPlatform>();
}
