using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;

public sealed class GetAuthenticationSettingsQueryHandler
    : IRequestHandler<
        GetAuthenticationSettingsQuery,
        AuthenticationSettingsResponse>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetAuthenticationSettingsQueryHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<AuthenticationSettingsResponse> Handle(
        GetAuthenticationSettingsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var application = await _db.Applications
            .AsNoTracking()
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
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ApplicationId ==
                         request.ApplicationId,
                    cancellationToken);

        if (settings is null)
        {
            throw new KeyNotFoundException(
                "Authentication settings not found.");
        }

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