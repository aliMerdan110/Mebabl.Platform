using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.DTOs;

namespace Mebabl.Platform.Application.Features.Projects.UpdateProject;

public sealed class UpdateProjectCommandHandler
    : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public UpdateProjectCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectDto> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");

        var developerId = _currentDeveloper.DeveloperId;

        var project = await _dbContext.Projects
            .Include(x => x.Applications)
            .FirstOrDefaultAsync(
                x => x.Id == request.ProjectId &&
                     x.DeveloperId == developerId,
                cancellationToken);

        if (project is null)
            throw new KeyNotFoundException("Project not found.");

        var normalizedCode = request.Code.Trim().ToLowerInvariant();

        var duplicate = await _dbContext.Projects
            .AnyAsync(
                x => x.Id != project.Id &&
                     x.DeveloperId == developerId &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (duplicate)
            throw new InvalidOperationException(
                "Project code already exists.");

        project.Name = request.Name.Trim();
        project.Code = normalizedCode;
        project.Description =
            string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
        project.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProjectDto(
            project.Id,
            project.DeveloperId,
            project.Name,
            project.Code,
            project.Description,
            project.IsActive,
            project.CreatedAt,
            project.UpdatedAt,
            project.Applications.Count);
    }
}