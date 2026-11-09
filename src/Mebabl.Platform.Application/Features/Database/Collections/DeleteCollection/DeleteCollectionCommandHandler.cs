using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Services.Clock;

namespace Mebabl.Platform.Application.Features.Database.Collections.DeleteCollection;

/// <summary>
/// Soft-deletes a collection within the project database resolved from the
/// authenticated application. ProjectId is resolved from trusted persistence
/// state and is never accepted from the request.
/// </summary>
public sealed class DeleteCollectionCommandHandler
    : IRequestHandler<DeleteCollectionCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IClock _clock;

    public DeleteCollectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IClock clock)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentApplication = currentApplication
            ?? throw new ArgumentNullException(nameof(currentApplication));
        _clock = clock
            ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task Handle(
        DeleteCollectionCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentApplication.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated trusted application is required.");
        }

        var applicationId = _currentApplication.ApplicationId;
        if (applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated application context is invalid.");
        }

        if (request.CollectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(request.CollectionId));
        }

        // Resolve the project from the authenticated application's database
        // record. Never trust a ProjectId supplied by the caller.
        var projectId = await _dbContext.Applications
            .AsNoTracking()
            .Where(application =>
                application.Id == applicationId &&
                application.IsActive &&
                !application.IsDeleted &&
                application.Project.IsActive &&
                !application.Project.IsDeleted)
            .Select(application => application.ProjectId)
            .FirstOrDefaultAsync(cancellationToken);

        if (projectId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The application or its project is unavailable.");
        }

        // Collections are project-scoped. ApplicationId identifies the
        // originating client, not the project data-isolation boundary.
        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(
                item =>
                    item.Id == request.CollectionId &&
                    item.ProjectId == projectId &&
                    !item.IsDeleted,
                cancellationToken);

        if (collection is null)
        {
            // Do not disclose whether the identifier belongs to another
            // project or has already been deleted.
            throw new KeyNotFoundException("Collection not found.");
        }

        if (collection.IsSystem)
        {
            throw new InvalidOperationException(
                "System collections cannot be deleted.");
        }

        // Soft deletion preserves documents for retention, recovery, and
        // audit workflows. Documents become inaccessible through the
        // collection's active/deleted state checks.
        collection.IsActive = false;
        collection.IsDeleted = true;
        collection.UpdatedAt = _clock.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
