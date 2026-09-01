using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Domain.Entities.Applications;

public sealed class ApplicationPlatform : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public PlatformApplication Application { get; set; } = default!;

    public string Platform { get; set; } = string.Empty;

    public string? Nickname { get; set; }

    public string? PackageName { get; set; }

    public string? BundleId { get; set; }

    public string? Domain { get; set; }

    public bool IsActive { get; set; } = true;

    
}