using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class ApplicationUser : AuditableEntity
{
    public Guid ApplicationId { get; set; }
    public PlatformApplication Application { get; set; } = default!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;


// تم اضافته بتاريخ 18/8
    public ICollection<ApplicationUserPasswordResetToken> PasswordResetTokens { get; set; } = new List<ApplicationUserPasswordResetToken>();

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}