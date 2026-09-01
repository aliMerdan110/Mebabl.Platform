using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public sealed class ApplicationUserEmailVerificationToken : AuditableEntity
{
    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = default!;

    public string TokenHash { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }
}