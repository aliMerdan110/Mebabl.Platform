
using Mebabl.Platform.Domain.Common.Entities;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class Developer : AuditableEntity
{

      
      public ICollection<PlatformProject> Projects { get; set; } = new List<PlatformProject>();

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