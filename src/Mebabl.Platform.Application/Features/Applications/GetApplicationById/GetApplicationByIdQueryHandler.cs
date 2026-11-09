using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.GetApplicationById;

public sealed class GetApplicationByIdQueryHandler
    : IRequestHandler<GetApplicationByIdQuery, GetApplicationByIdResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetApplicationByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<GetApplicationByIdResponse> Handle(
        GetApplicationByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var application = await _dbContext.Applications
            .AsNoTracking()
            .Where(x =>
                x.Id == request.Id &&
                x.Project.DeveloperId == _currentDeveloper.DeveloperId)
            .Select(x => new GetApplicationByIdResponse(
                x.Id,
                x.ProjectId,
                x.ProjectPlatformId,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.ProjectPlatform.Platform,
                x.ProjectPlatform.Nickname,
                x.ProjectPlatform.PackageName,
                x.ProjectPlatform.BundleId,
                x.ProjectPlatform.Domain))
            .SingleOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        return application;
    }
}