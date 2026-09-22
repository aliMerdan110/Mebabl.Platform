using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.GetProject;

public sealed class GetProjectQueryHandler
    : IRequestHandler<GetProjectQuery, ProjectDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetProjectQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectDto> Handle(
        GetProjectQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");

        var developerId = _currentDeveloper.DeveloperId;

        var project = await _dbContext.Projects
            .AsNoTracking()
            .Where(x =>
                x.Id == request.ProjectId &&
                x.DeveloperId == developerId)
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
            .FirstOrDefaultAsync(cancellationToken);

        return project
            ?? throw new KeyNotFoundException("Project not found.");
    }
}