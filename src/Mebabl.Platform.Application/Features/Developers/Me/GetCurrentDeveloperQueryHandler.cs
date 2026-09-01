using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Developers.Me;

public sealed class GetCurrentDeveloperQueryHandler
    : IRequestHandler<GetCurrentDeveloperQuery, GetCurrentDeveloperResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetCurrentDeveloperQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<GetCurrentDeveloperResponse> Handle(
        GetCurrentDeveloperQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        var developer = await _dbContext.Developers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (developer is null)
            throw new Exception("Developer not found.");

        return new GetCurrentDeveloperResponse(
            developer.Id,
            developer.DisplayName,
            developer.Email,
            developer.IsActive);
    }
}