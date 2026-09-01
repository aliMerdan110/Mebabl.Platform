using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;

public sealed class UpdateAuthenticationSettingsCommandHandler
    : IRequestHandler<
        UpdateAuthenticationSettingsCommand,
        AuthenticationSettingsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public UpdateAuthenticationSettingsCommandHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<AuthenticationSettingsResponse> Handle(
        UpdateAuthenticationSettingsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var application = await _db.Applications
            .FirstOrDefaultAsync(
                x => x.Id == request.ApplicationId,
                cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException(
                "Application not found.");
        }

        if (application.DeveloperId !=
            _currentDeveloper.DeveloperId)
        {
            throw new UnauthorizedAccessException(
                "You do not have access to this application.");
        }

        var settings =
            await _db.ApplicationAuthenticationSettings
                .FirstOrDefaultAsync(
                    x => x.ApplicationId ==
                         request.ApplicationId,
                    cancellationToken);

        if (settings is null)
        {
            settings =
                new Domain.Entities.Identity
                    .ApplicationAuthenticationSettings
                {
                    ApplicationId =
                        request.ApplicationId,

                    AllowRegistration =
                        request.AllowRegistration,

                    RequireEmailVerification =
                        request.RequireEmailVerification,

                    AllowPasswordAuthentication =
                        request.AllowPasswordAuthentication,

                    AllowAnonymousAuthentication =
                        request.AllowAnonymousAuthentication,

                    PasswordMinLength =
                        request.PasswordMinLength,

                    SessionLifetimeDays =
                        request.SessionLifetimeDays,

                    RefreshTokenLifetimeDays =
                        request.RefreshTokenLifetimeDays,

                    MaxLoginAttempts =
                        request.MaxLoginAttempts
                };

            _db.ApplicationAuthenticationSettings.Add(
                settings);
        }
        else
        {
            settings.AllowRegistration =
                request.AllowRegistration;

            settings.RequireEmailVerification =
                request.RequireEmailVerification;

            settings.AllowPasswordAuthentication =
                request.AllowPasswordAuthentication;

            settings.AllowAnonymousAuthentication =
                request.AllowAnonymousAuthentication;

            settings.PasswordMinLength =
                request.PasswordMinLength;

            settings.SessionLifetimeDays =
                request.SessionLifetimeDays;

            settings.RefreshTokenLifetimeDays =
                request.RefreshTokenLifetimeDays;

            settings.MaxLoginAttempts =
                request.MaxLoginAttempts;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthenticationSettingsResponse(
            settings.Id,
            settings.ApplicationId,
            settings.AllowRegistration,
            settings.RequireEmailVerification,
            settings.AllowPasswordAuthentication,
            settings.AllowAnonymousAuthentication,
            settings.PasswordMinLength,
            settings.SessionLifetimeDays,
            settings.RefreshTokenLifetimeDays,
            settings.MaxLoginAttempts);
    }
}