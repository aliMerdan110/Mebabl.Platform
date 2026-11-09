using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatforms;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.GetProjectPlatform;

public sealed class GetProjectPlatformQueryHandler
    : IRequestHandler<GetProjectPlatformQuery, ProjectPlatformDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetProjectPlatformQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectPlatformDto> Handle(
        GetProjectPlatformQuery request,
        CancellationToken cancellationToken)
    {
        var platform = await _dbContext.ProjectPlatforms
            .AsNoTracking()
            .Where(x =>
                x.Id == request.PlatformId &&
                x.ProjectId == request.ProjectId &&
                x.Project.DeveloperId == _currentDeveloper.DeveloperId)
            .Select(x => new ProjectPlatformDto(
                x.Id,
                x.ProjectId,
                x.Platform,
                x.Nickname,
                x.PackageName,
                x.BundleId,
                x.Domain,
                x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);

        return platform
            ?? throw new KeyNotFoundException(
                "Project platform was not found.");
    }
}