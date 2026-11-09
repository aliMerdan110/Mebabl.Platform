using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.GetCollection;

/// <summary>
/// Reads collection metadata within the authenticated project's resource boundary.
/// </summary>
public sealed class GetCollectionQueryHandler
    : IRequestHandler<GetCollectionQuery, CollectionResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public GetCollectionQueryHandler(
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

    public async Task<CollectionResponse> Handle(
        GetCollectionQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated project user is required.");
        }

        var userId = _currentUser.UserId;
        var accountId = _currentUser.AccountId;
        var applicationId = _currentUser.ApplicationId;
        var projectId = _currentUser.ProjectId;

        if (userId == Guid.Empty ||
            accountId == Guid.Empty ||
            applicationId == Guid.Empty ||
            projectId == Guid.Empty)
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

        var collection = await _dbContext.Collections
            .AsNoTracking()
            .Where(collection =>
                collection.Id == request.CollectionId &&
                collection.ProjectId == projectId &&
                collection.IsActive &&
                !collection.IsDeleted &&
                collection.Project.IsActive &&
                !collection.Project.IsDeleted)
            .Select(collection => new CollectionResponse(
                collection.Id,
                collection.ProjectId,
                collection.Name,
                collection.Description,
                collection.IsActive,
                collection.CreatedAt,
                collection.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (collection is null)
        {
            // Do not disclose whether a collection exists in another project.
            throw new KeyNotFoundException("Collection not found.");
        }

        await _security.EnsureReadAsync(
            request.CollectionId,
            cancellationToken);

        return collection;
    }
}
