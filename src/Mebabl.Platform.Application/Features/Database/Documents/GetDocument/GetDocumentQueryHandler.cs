using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.GetDocument;

/// <summary>
/// Retrieves a document within the authenticated project's data boundary.
/// Creator identifiers are provenance metadata, not access boundaries.
/// </summary>
public sealed class GetDocumentQueryHandler
    : IRequestHandler<GetDocumentQuery, DocumentResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public GetDocumentQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _security = security ?? throw new ArgumentNullException(nameof(security));
    }

    public async Task<DocumentResponse> Handle(
        GetDocumentQuery request,
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
        var accountId = _currentUser.AccountId;

        if (projectId == Guid.Empty ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            accountId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated project user context is invalid.");
        }

        if (request.DocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Document ID is required.",
                nameof(request.DocumentId));
        }

        var document = await _dbContext.Documents
            .AsNoTracking()
            .Where(document =>
                document.Id == request.DocumentId &&
                document.ProjectId == projectId &&
                !document.IsDeleted &&
                document.Collection.ProjectId == projectId &&
                document.Project.Id == projectId &&
                document.Project.IsActive &&
                !document.Project.IsDeleted &&
                document.Collection.IsActive &&
                !document.Collection.IsDeleted &&
                document.Collection.Project.IsActive &&
                !document.Collection.Project.IsDeleted)
            .Select(document => new
            {
                document.Id,
                document.Key,
                document.Data,
                document.Version,
                document.CreatedAt,
                document.UpdatedAt,
                document.CollectionId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        await _security.EnsureReadAsync(document.CollectionId, cancellationToken);

        return new DocumentResponse(
            document.Id,
            document.Key,
            document.Data,
            document.Version,
            document.CreatedAt,
            document.UpdatedAt);
    }
}
