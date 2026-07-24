namespace Mebabl.Platform.Domain.Entities.Identity;

public class RefreshToken
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid ApplicationUserId { get; set; }
    public ApplicationUser User { get; set; } = default!;

    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsRevoked => RevokedAt != null;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}