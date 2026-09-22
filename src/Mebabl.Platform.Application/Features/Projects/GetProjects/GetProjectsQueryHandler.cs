using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.GetProjects;

public sealed class GetProjectsQueryHandler
    : IRequestHandler<GetProjectsQuery, IReadOnlyList<ProjectDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetProjectsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<IReadOnlyList<ProjectDto>> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");

        var developerId = _currentDeveloper.DeveloperId;

        return await _dbContext.Projects
            .AsNoTracking()
            .Where(x => x.DeveloperId == developerId)
            .OrderBy(x => x.Name)
            .Select(x => new ProjectDto(
                x.Id,
                x.DeveloperId,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt,
                x.Applications.Count))
            .ToListAsync(cancellationToken);
    }
}