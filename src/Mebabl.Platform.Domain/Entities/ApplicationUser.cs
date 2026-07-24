using Mebabl.Platform.Domain.Entities.Identity;

namespace Mebabl.Platform.Domain.Entities;

public class ApplicationUser
{
    public Guid Id { get; set; }
    
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = default!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = default!;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

