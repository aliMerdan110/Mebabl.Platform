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
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");

        var developerId = _currentDeveloper.DeveloperId;

        var project = await _dbContext.Projects
            .FirstOrDefaultAsync(
                x => x.Id == request.ProjectId &&
                     x.DeveloperId == developerId,
                cancellationToken);

        if (project is null)
            throw new KeyNotFoundException("Project not found.");

        _dbContext.Projects.Remove(project);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}