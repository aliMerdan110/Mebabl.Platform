using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.GetCollection;

public sealed class GetCollectionQueryHandler
    : IRequestHandler<GetCollectionQuery, CollectionResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetCollectionQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<CollectionResponse> Handle(
        GetCollectionQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var applicationId = _currentUser.ApplicationId;

        var collection = await _dbContext.Collections
            .AsNoTracking()
            .Where(x =>
                x.Id == request.CollectionId &&
                x.ApplicationId == applicationId)
            .Select(x => new CollectionResponse(
                x.Id,
                x.ApplicationId,
                x.Name,
                x.Description,
                x.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (collection is null)
            throw new KeyNotFoundException("Collection not found.");

        return collection;
    }
}