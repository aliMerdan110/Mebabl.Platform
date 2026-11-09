using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Projects.DeleteProject;

public sealed class DeleteProjectCommandHandler
    : IRequestHandler<DeleteProjectCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public DeleteProjectCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var developerId = _currentDeveloper.DeveloperId;

        var project = await _dbContext.Projects
            .SingleOrDefaultAsync(
                x => x.Id == request.ProjectId
                     && x.DeveloperId == developerId,
                cancellationToken);

        if (project is null)
        {
            throw new KeyNotFoundException("Project not found.");
        }

        var hasApplications = await _dbContext.Applications
            .AnyAsync(
                x => x.ProjectId == project.Id,
                cancellationToken);

        if (hasApplications)
        {
            throw new InvalidOperationException(
                "The project cannot be deleted while applications exist. " +
                "Delete the applications and their dependent data first.");
        }

        await using var transaction =
            await _dbContext.BeginTransactionAsync(cancellationToken);

        try
        {
            var sdkConfiguration =
                await _dbContext.ProjectSdkConfigurations
                    .SingleOrDefaultAsync(
                        x => x.ProjectId == project.Id,
                        cancellationToken);

            if (sdkConfiguration is not null)
            {
                _dbContext.ProjectSdkConfigurations.Remove(
                    sdkConfiguration);
            }

            var platforms = await _dbContext.ProjectPlatforms
                .Where(x => x.ProjectId == project.Id)
                .ToListAsync(cancellationToken);

            if (platforms.Count > 0)
            {
                _dbContext.ProjectPlatforms.RemoveRange(platforms);
            }

            _dbContext.Projects.Remove(project);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}