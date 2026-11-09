using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.ListDocuments;

/// <summary>
/// Lists documents from a collection inside the authenticated project.
/// Both document and collection project identifiers are enforced explicitly.
/// </summary>
public sealed class ListDocumentsQueryHandler
    : IRequestHandler<ListDocumentsQuery, IReadOnlyList<DocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public ListDocumentsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _security = security
            ?? throw new ArgumentNullException(nameof(security));
    }

    public async Task<IReadOnlyList<DocumentResponse>> Handle(
        ListDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated project user is required.");
        }

        var projectId = _currentUser.ProjectId;
        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;

        if (projectId == Guid.Empty ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated project user context is invalid.");
        }

        if (request.CollectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(request.CollectionId));
        }

        var collectionExists = await _dbContext.Collections
            .AsNoTracking()
            .AnyAsync(
                collection =>
                    collection.Id == request.CollectionId &&
                    collection.ProjectId == projectId &&
                    collection.IsActive &&
                    !collection.IsDeleted &&
                    collection.Project.IsActive &&
                    !collection.Project.IsDeleted,
                cancellationToken);

        if (!collectionExists)
        {
            // Do not disclose whether a collection exists in another project.
            throw new KeyNotFoundException("Collection not found.");
        }

        await _security.EnsureReadAsync(
            request.CollectionId,
            cancellationToken);

        return await _dbContext.Documents
            .AsNoTracking()
            .Where(document =>
                document.ProjectId == projectId &&
                document.CollectionId == request.CollectionId &&
                !document.IsDeleted &&
                document.Collection.ProjectId == projectId &&
                document.Collection.IsActive &&
                !document.Collection.IsDeleted &&
                document.Collection.Project.IsActive &&
                !document.Collection.Project.IsDeleted)
            .OrderByDescending(document => document.CreatedAt)
            .ThenByDescending(document => document.Id)
            .Select(document => new DocumentResponse(
                document.Id,
                document.Key,
                document.Data,
                document.Version,
                document.CreatedAt,
                document.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
