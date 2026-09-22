using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Projects.DTOs;
using Mebabl.Platform.Domain.Entities.Projects;

namespace Mebabl.Platform.Application.Features.Projects.CreateProject;

public sealed class CreateProjectCommandHandler
    : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public CreateProjectCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task<ProjectDto> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");

        var developerId = _currentDeveloper.DeveloperId;

        var normalizedCode = request.Code.Trim().ToLowerInvariant();

        var exists = await _dbContext.Projects
            .AnyAsync(
                x => x.DeveloperId == developerId &&
                     x.Code == normalizedCode,
                cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                "Project code already exists.");

        var project = new PlatformProject
        {
            DeveloperId = developerId,
            Name = request.Name.Trim(),
            Code = normalizedCode,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            IsActive = true
        };

        _dbContext.Projects.Add(project);

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
            0);
    }
}