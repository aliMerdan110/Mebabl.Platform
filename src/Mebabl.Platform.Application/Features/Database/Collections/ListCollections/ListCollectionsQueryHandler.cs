using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.ListCollections;

public sealed class ListCollectionsQueryHandler
    : IRequestHandler<ListCollectionsQuery, IReadOnlyList<CollectionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public ListCollectionsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<CollectionResponse>> Handle(
        ListCollectionsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var applicationId = _currentUser.ApplicationId;

        return await _dbContext.Collections
            .AsNoTracking()
            .Where(x =>
                x.ApplicationId == applicationId &&
                x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new CollectionResponse(
                x.Id,
                x.ApplicationId,
                x.Name,
                x.Description,
                x.IsActive,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}