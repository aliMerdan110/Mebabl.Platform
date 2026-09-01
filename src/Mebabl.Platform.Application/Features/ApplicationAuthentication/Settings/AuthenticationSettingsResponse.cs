namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;

public sealed record AuthenticationSettingsResponse(
    Guid Id,
    Guid ApplicationId,
    bool AllowRegistration,
    bool RequireEmailVerification,
    bool AllowPasswordAuthentication,
    bool AllowAnonymousAuthentication,
    int PasswordMinLength,
    int SessionLifetimeDays,
    int RefreshTokenLifetimeDays,
    int MaxLoginAttempts);