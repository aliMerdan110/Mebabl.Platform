
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.DeleteProjectPlatform;

public sealed class DeleteProjectPlatformCommandHandler
    : IRequestHandler<DeleteProjectPlatformCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public DeleteProjectPlatformCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        DeleteProjectPlatformCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var platform = await _dbContext.ProjectPlatforms
            .FirstOrDefaultAsync(
                x => x.Id == request.PlatformId
                    && x.ProjectId == request.ProjectId
                    && x.Project.DeveloperId == _currentDeveloper.DeveloperId
                    && !x.Project.IsDeleted,
                cancellationToken);

        if (platform is null)
        {
            throw new KeyNotFoundException(
                "Project platform not found.");
        }

        _dbContext.ProjectPlatforms.Remove(platform);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
