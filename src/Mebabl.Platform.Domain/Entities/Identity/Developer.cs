
using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class Developer : AuditableEntity
{
    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<PlatformApplication> Applications { get; set; }
        = new List<PlatformApplication>();

    public ICollection<DeveloperRefreshToken> RefreshTokens { get; set; }
        = new List<DeveloperRefreshToken>();

    public ICollection<DeveloperPasswordResetToken> PasswordResetTokens { get; set; }
        = new List<DeveloperPasswordResetToken>();
}