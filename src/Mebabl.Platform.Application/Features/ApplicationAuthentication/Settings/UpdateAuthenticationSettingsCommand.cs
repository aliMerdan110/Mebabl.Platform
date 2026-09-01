using MediatR;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;

public sealed record UpdateAuthenticationSettingsCommand(
    Guid ApplicationId,
    bool AllowRegistration,
    bool RequireEmailVerification,
    bool AllowPasswordAuthentication,
    bool AllowAnonymousAuthentication,
    int PasswordMinLength,
    int SessionLifetimeDays,
    int RefreshTokenLifetimeDays,
    int MaxLoginAttempts
) : IRequest<AuthenticationSettingsResponse>;