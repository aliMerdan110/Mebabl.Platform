using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;

public sealed class ToggleAuthProviderCommandHandler
    : IRequestHandler<ToggleAuthProviderCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public ToggleAuthProviderCommandHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        ToggleAuthProviderCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationExists = await _db.Applications
            .AnyAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!applicationExists)
            throw new KeyNotFoundException(
                "Application was not found.");

        var provider = await _db.ApplicationAuthProviders
            .FirstOrDefaultAsync(
                x =>
                    x.ApplicationId == request.ApplicationId &&
                    x.Provider == request.Provider,
                cancellationToken);

        if (provider is null)
            throw new KeyNotFoundException(
                "Authentication provider not found.");

        provider.IsEnabled = request.IsEnabled;

        await _db.SaveChangesAsync(cancellationToken);
    }
}