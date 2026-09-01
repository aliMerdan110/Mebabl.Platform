using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public sealed class ApplicationUserPasswordResetToken : AuditableEntity
{
    public Guid UserId { get; set; }
    
    // خاصية الربط (Navigation Property) مع مستخدم الـ SDK
    public ApplicationUser User { get; set; } = default!;

    public string TokenHash { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }
}