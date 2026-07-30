using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class Tenant  : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    public ICollection<PlatformApplication> Applications { get; set; }
    = new List<PlatformApplication>();
}

