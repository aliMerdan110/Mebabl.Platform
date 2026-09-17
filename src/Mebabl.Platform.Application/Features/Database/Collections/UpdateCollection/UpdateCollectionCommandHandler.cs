using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;



// تم التعديل 11/9  
public sealed class UpdateCollectionCommandHandler
    : IRequestHandler<UpdateCollectionCommand, CollectionResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public UpdateCollectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<CollectionResponse> Handle(
        UpdateCollectionCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var applicationId = _currentUser.ApplicationId;
        var name = request.Name.Trim();

        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.CollectionId &&
                    x.ApplicationId == applicationId,
                cancellationToken);

        if (collection is null)
            throw new KeyNotFoundException("Collection not found.");

        var exists = await _dbContext.Collections.AnyAsync(
            x =>
                x.Id != request.CollectionId &&
                x.ApplicationId == applicationId &&
                x.Name.ToLower() == name.ToLower(),
            cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                "Collection already exists.");

        var code = GenerateCode(name);

        var codeExists = await _dbContext.Collections.AnyAsync(
            x =>
                x.Id != request.CollectionId &&
                x.ApplicationId == applicationId &&
                x.Code == code,
            cancellationToken);

        if (codeExists)
        {
            code = $"{code}_{Guid.NewGuid():N}";
        }

        collection.Name = name;
        collection.Code = code;
        collection.Description =
            request.Description?.Trim() ?? string.Empty;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionResponse(
            collection.Id,
            collection.ApplicationId,
            collection.Name,
            collection.Description,
            collection.IsActive,
            collection.CreatedAt,
collection.UpdatedAt);
    }

    private static string GenerateCode(string name)
    {
        var code = new string(
            name
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) ? c : '_')
                .ToArray());

        while (code.Contains("__"))
            code = code.Replace("__", "_");

        return code.Trim('_');
    }
}