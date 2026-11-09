using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class DeveloperRefreshToken : AuditableEntity
{
    public Guid DeveloperId { get; set; }

    public Developer Developer { get; set; } = default!;

    // Legacy: retained temporarily to support existing tokens.
    public string? Token { get; set; }

    // SHA-256 hash for newly issued refresh tokens.
    public string? TokenHash { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}