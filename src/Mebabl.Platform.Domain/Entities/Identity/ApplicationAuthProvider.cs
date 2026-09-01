using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class ApplicationAuthProvider : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public PlatformApplication Application { get; set; } = default!;

    public string Provider { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public string? ConfigurationJson { get; set; }
}