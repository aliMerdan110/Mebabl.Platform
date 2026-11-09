using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Applications.DeleteApplication;

public sealed class DeleteApplicationCommandHandler
    : IRequestHandler<DeleteApplicationCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public DeleteApplicationCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext;
        _currentDeveloper = currentDeveloper;
    }

    public async Task Handle(
        DeleteApplicationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authenticated developer is required.");
        }

        var application = await _dbContext.Applications
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.Project.DeveloperId == _currentDeveloper.DeveloperId,
                cancellationToken);

        if (application is null)
        {
            throw new KeyNotFoundException("Application not found.");
        }

        var hasUsers = await _dbContext.ApplicationUsers
            .AnyAsync(
                x => x.ApplicationId == application.Id,
                cancellationToken);

        if (hasUsers)
        {
            throw new InvalidOperationException(
                "The application cannot be deleted while application users exist.");
        }

        _dbContext.Applications.Remove(application);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}