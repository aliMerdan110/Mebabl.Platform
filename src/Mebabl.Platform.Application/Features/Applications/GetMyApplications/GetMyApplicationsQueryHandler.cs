using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.GetMyApplications;

public sealed class GetMyApplicationsQueryHandler
    : IRequestHandler<GetMyApplicationsQuery, IReadOnlyList<ApplicationItemResponse>>
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
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var developerId = _currentDeveloper.DeveloperId;

        if (request.ProjectId.HasValue)
        {
            var ownsProject = await _dbContext.Projects
                .AsNoTracking()
                .AnyAsync(
                    project =>
                        project.Id == request.ProjectId.Value &&
                        project.DeveloperId == developerId,
                    cancellationToken);

            if (!ownsProject)
            {
                throw new KeyNotFoundException("Project not found.");
            }
        }

        return await _dbContext.Applications
            .AsNoTracking()
            .Where(application =>
                application.Project.DeveloperId == developerId &&
                (!request.ProjectId.HasValue ||
                 application.ProjectId == request.ProjectId.Value))
            .OrderBy(application => application.CreatedAt)
            .Select(application => new ApplicationItemResponse(
                application.Id,
                application.Name,
                application.Code,
                application.Description,
                application.IsActive,
                application.ProjectPlatformId,
                application.ProjectPlatform.Platform,
                application.ProjectPlatform.Nickname,
                application.ProjectPlatform.PackageName,
                application.ProjectPlatform.BundleId,
                application.ProjectPlatform.Domain,
                application.CreatedAt,
                application.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}