using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Clock;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

/// <summary>
/// Soft-deletes a document in the authenticated project using an atomic
/// version-checked update. Authorization is delegated to the collection policy.
/// </summary>
public sealed class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;
    private readonly IClock _clock;

    public DeleteDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security,
        IClock clock)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _security = security ?? throw new ArgumentNullException(nameof(security));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        var projectId = _currentUser.ProjectId;
        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;
        var accountId = _currentUser.AccountId;

        if (projectId == Guid.Empty ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            accountId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated project user scope is invalid.");
        }

        if (request.CollectionId == Guid.Empty || request.DocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID and document ID are required.");
        }

        if (request.ExpectedVersion < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.ExpectedVersion),
                "Expected version must be a positive integer.");
        }

        // Read the version inside the project boundary. The nullable projection
        // distinguishes a missing resource from a valid version value.
        var currentVersion = await _dbContext.Documents
            .AsNoTracking()
            .Where(document =>
                document.Id == request.DocumentId &&
                document.CollectionId == request.CollectionId &&
                document.ProjectId == projectId &&
                !document.IsDeleted &&
                document.Collection.ProjectId == projectId &&
                document.Collection.IsActive &&
                !document.Collection.IsDeleted &&
                document.Project.IsActive &&
                !document.Project.IsDeleted)
            .Select(document => (int?)document.Version)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentVersion is null)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        await _security.EnsureDeleteAsync(request.CollectionId, cancellationToken);

        if (currentVersion.Value != request.ExpectedVersion)
        {
            throw new InvalidOperationException(
                "Document version conflict. Reload the document before retrying.");
        }

        if (currentVersion.Value == int.MaxValue)
        {
            throw new InvalidOperationException(
                "The document version limit has been reached.");
        }

        // ExecuteUpdate bypasses EF's tracked concurrency handling, so the
        // expected version is explicitly included in the atomic predicate.
        var now = _clock.UtcNow;
        var affectedRows = await _dbContext.Documents
            .Where(document =>
                document.Id == request.DocumentId &&
                document.CollectionId == request.CollectionId &&
                document.ProjectId == projectId &&
                document.Version == request.ExpectedVersion &&
                !document.IsDeleted &&
                document.Collection.ProjectId == projectId &&
                document.Collection.IsActive &&
                !document.Collection.IsDeleted &&
                document.Project.IsActive &&
                !document.Project.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(document => document.IsDeleted, true)
                    .SetProperty(
                        document => document.Version,
                        document => document.Version + 1)
                    .SetProperty(
                        document => document.UpdatedAt,
                        (DateTime?)now),
                cancellationToken);

        if (affectedRows != 1)
        {
            throw new InvalidOperationException(
                "Document version conflict. Reload the document before retrying.");
        }
    }
}
