using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;

public sealed class GetAuthProvidersQueryHandler
    : IRequestHandler<
        GetAuthProvidersQuery,
        IReadOnlyList<AuthProviderResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetAuthProvidersQueryHandler(
        IApplicationDbContext db,
        ICurrentDeveloper currentDeveloper)
    {
        _db = db;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<IReadOnlyList<AuthProviderResponse>> Handle(
        GetAuthProvidersQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var applicationExists = await _db.Applications
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.Id == request.ApplicationId &&
                    x.DeveloperId == _currentDeveloper.DeveloperId &&
                    !x.IsDeleted,
                cancellationToken);

        if (!applicationExists)
            throw new KeyNotFoundException(
                "Application was not found.");

        return await _db.ApplicationAuthProviders
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == request.ApplicationId)
            .OrderBy(x => x.Provider)
            .Select(x => new AuthProviderResponse(
                x.Id,
                x.Provider,
                x.IsEnabled))
            .ToListAsync(cancellationToken);
    }
}