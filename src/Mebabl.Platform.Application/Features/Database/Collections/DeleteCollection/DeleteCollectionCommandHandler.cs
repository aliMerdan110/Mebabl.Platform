using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Database.Collections.DeleteCollection;

public sealed class DeleteCollectionCommandHandler
    : IRequestHandler<DeleteCollectionCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DeleteCollectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeleteCollectionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CollectionId &&
                    x.ApplicationId == _currentUser.ApplicationId,
                cancellationToken);

        if (collection is null)
            throw new KeyNotFoundException("Collection not found.");

        collection.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}