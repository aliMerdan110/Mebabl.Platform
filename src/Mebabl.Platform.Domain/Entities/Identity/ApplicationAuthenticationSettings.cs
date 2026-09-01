using Mebabl.Platform.Domain.Common.Entities;

namespace Mebabl.Platform.Domain.Entities.Identity;

public class ApplicationAuthenticationSettings : AuditableEntity
{
    public Guid ApplicationId { get; set; }

    public PlatformApplication Application { get; set; } = default!;

    public bool AllowRegistration { get; set; } = true;

    public bool RequireEmailVerification { get; set; } = false;

    public bool AllowPasswordAuthentication { get; set; } = true;

    public bool AllowAnonymousAuthentication { get; set; } = false;

    public int PasswordMinLength { get; set; } = 8;

    public int SessionLifetimeDays { get; set; } = 7;

    public int RefreshTokenLifetimeDays { get; set; } = 30;

    public int MaxLoginAttempts { get; set; } = 5;
}