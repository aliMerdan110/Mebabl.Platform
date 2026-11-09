using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Identity;

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
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var application = await _db.Applications
            .AsNoTracking()
            .Where(x =>
                x.Id == request.ApplicationId &&
                x.Project.DeveloperId == _currentDeveloper.DeveloperId &&
                !x.IsDeleted)
            .Select(x => new { x.Id })
            .SingleOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        var settings = await _db.ApplicationAuthenticationSettings
            .FirstOrDefaultAsync(
                x => x.ApplicationId == application.Id,
                cancellationToken);

        if (settings is null)
        {
            settings = new ApplicationAuthenticationSettings
            {
                ApplicationId = application.Id
            };

            _db.ApplicationAuthenticationSettings.Add(settings);
        }

        settings.AllowRegistration = request.AllowRegistration;
        settings.RequireEmailVerification = request.RequireEmailVerification;
        settings.AllowPasswordAuthentication = request.AllowPasswordAuthentication;
        settings.AllowAnonymousAuthentication = request.AllowAnonymousAuthentication;
        settings.PasswordMinLength = request.PasswordMinLength;
        settings.SessionLifetimeDays = request.SessionLifetimeDays;
        settings.RefreshTokenLifetimeDays = request.RefreshTokenLifetimeDays;
        settings.MaxLoginAttempts = request.MaxLoginAttempts;

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