using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.GetMyApplications;

public sealed class GetMyApplicationsQueryHandler
    : IRequestHandler<
        GetMyApplicationsQuery,
        IReadOnlyList<ApplicationItemResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetMyApplicationsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<IReadOnlyList<ApplicationItemResponse>> Handle(
        GetMyApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await _dbContext.Applications
            .AsNoTracking()
            .Where(x => x.DeveloperId == _currentDeveloper.DeveloperId)
            .OrderBy(x => x.Name)
            .Select(x => new ApplicationItemResponse(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.Domain,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}