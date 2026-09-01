using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;

public sealed class CreateCollectionCommandHandler
    : IRequestHandler<CreateCollectionCommand, CollectionResponse>
{
    private readonly IApplicationDbContext _dbContext;

    private readonly ICurrentUser _currentUser;

    public CreateCollectionCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUser currentUser)
{
    _dbContext = dbContext;
    _currentUser = currentUser;
}

    public async Task<CollectionResponse> Handle(
        CreateCollectionCommand request,
        CancellationToken cancellationToken)
    {
         
        if (!_currentUser.IsAuthenticated ||
    _currentUser.ApplicationId == Guid.Empty)
{
    throw new UnauthorizedAccessException();
}

var applicationId = _currentUser.ApplicationId;
        var exists = await _dbContext.Collections.AnyAsync(
    x => x.ApplicationId == applicationId &&
         x.Name == request.Name,
    cancellationToken);

        if (exists)
            throw new Exception("Collection already exists.");

        var collection = new Collection
        {
            ApplicationId = applicationId,
            Name = request.Name,
            Description = request.Description
        };

        _dbContext.Collections.Add(collection);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionResponse(
            collection.Id,
            collection.ApplicationId,
            collection.Name,
            collection.Description,
            collection.IsActive);
    }
}