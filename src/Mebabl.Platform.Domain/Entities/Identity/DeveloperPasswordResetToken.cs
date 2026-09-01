
using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class DeveloperPasswordResetToken : AuditableEntity
{
    public Guid DeveloperId { get; set; }

    public Developer Developer { get; set; } = null!;

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public bool IsUsed => UsedAt.HasValue;
}