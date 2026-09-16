using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class ApplicationUser : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public PlatformApplication Application { get; set; } = default!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = default!;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string NormalizedUsername { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string SecurityStamp { get; set; } =
        Guid.NewGuid().ToString();

    public bool EmailConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public bool LockoutEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public int AccessFailedCount { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public ICollection<ApplicationUserPasswordResetToken>
        PasswordResetTokens { get; set; }
        = new List<ApplicationUserPasswordResetToken>();

    public ICollection<ApplicationUserEmailVerificationToken>
        EmailVerificationTokens { get; set; }
        = new List<ApplicationUserEmailVerificationToken>();

    public ICollection<ApplicationUserRole> UserRoles { get; set; }
        = new List<ApplicationUserRole>();

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}