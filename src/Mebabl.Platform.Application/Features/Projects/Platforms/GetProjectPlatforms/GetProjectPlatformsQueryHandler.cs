
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;

public sealed class GetProjectPlatformsQueryHandler
    : IRequestHandler<GetProjectPlatformsQuery, IReadOnlyList<ProjectPlatformDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetProjectPlatformsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<IReadOnlyList<ProjectPlatformDto>> Handle(
        GetProjectPlatformsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var projectExists = await _dbContext.Projects
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == request.ProjectId
                    && x.DeveloperId == _currentDeveloper.DeveloperId
                    && !x.IsDeleted,
                cancellationToken);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        return await _dbContext.ProjectPlatforms
            .AsNoTracking()
            .Where(x => x.ProjectId == request.ProjectId)
            .OrderBy(x => x.Platform)
            .Select(x => new ProjectPlatformDto(
                x.Id,
                x.ProjectId,
                x.Platform,
                x.Nickname,
                x.PackageName,
                x.BundleId,
                x.Domain,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}
