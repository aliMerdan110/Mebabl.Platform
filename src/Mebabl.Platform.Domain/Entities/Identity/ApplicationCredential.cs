using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class ApplicationCredential : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public PlatformApplication Application { get; set; } = default!;

    public string ApiKey { get; set; } = string.Empty;

    public string ApiSecretHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}