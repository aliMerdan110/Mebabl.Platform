using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class RefreshToken : AuditableEntity
{
    public Guid ApplicationUserId { get; set; }

    public ApplicationUser ApplicationUser { get; set; } = default!;

    // Legacy: retained temporarily to support existing tokens.
    public string? Token { get; set; }

    // SHA-256 hash for newly issued refresh tokens.
    public string? TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}