using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.ListCollections;

/// <summary>
/// Lists active collections owned by the authenticated project and exposes
/// only collections whose published policy grants read access.
/// </summary>
public sealed class ListCollectionsQueryHandler
    : IRequestHandler<ListCollectionsQuery, IReadOnlyList<CollectionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public ListCollectionsQueryHandler(
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

    public async Task<IReadOnlyList<CollectionResponse>> Handle(
        ListCollectionsQuery request,
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

        var collections = await _dbContext.Collections
            .AsNoTracking()
            .Where(collection =>
                collection.ProjectId == projectId &&
                collection.IsActive &&
                !collection.IsDeleted &&
                collection.Project.IsActive &&
                !collection.Project.IsDeleted)
            .OrderBy(collection => collection.Name)
            .ThenBy(collection => collection.Id)
            .Select(collection => new CollectionResponse(
                collection.Id,
                collection.ProjectId,
                collection.Name,
                collection.Description,
                collection.IsActive,
                collection.CreatedAt,
                collection.UpdatedAt))
            .ToListAsync(cancellationToken);

        if (collections.Count == 0)
        {
            return collections;
        }

        var readableCollectionIds =
            await _security.GetReadableCollectionIdsAsync(
                collections.Select(collection => collection.Id).ToArray(),
                cancellationToken);

        return collections
            .Where(collection => readableCollectionIds.Contains(collection.Id))
            .ToArray();
    }
}
