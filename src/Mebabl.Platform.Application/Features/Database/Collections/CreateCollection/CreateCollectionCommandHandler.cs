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
        var name = request.Name.Trim();

        var exists = await _dbContext.Collections.AnyAsync(
            x => x.ApplicationId == applicationId &&
                 x.Name.ToLower() == name.ToLower(),
            cancellationToken);

        if (exists)
            throw new InvalidOperationException(
                "Collection already exists.");

        var code = GenerateCode(name);

        var codeExists = await _dbContext.Collections.AnyAsync(
            x => x.ApplicationId == applicationId &&
                 x.Code == code,
            cancellationToken);

        if (codeExists)
        {
            code = $"{code}_{Guid.NewGuid():N}";
        }

        var collection = new Collection
        {
            ApplicationId = applicationId,
            Name = name,
            Code = code,
            Description = request.Description?.Trim() ?? string.Empty
        };

        _dbContext.Collections.Add(collection);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create default security rules for the new collection.
        var permissions = new[]
        {
            "read",
            "write",
            "delete",
            "query"
        };

        foreach (var permission in permissions)
        {
            var rule = new SecurityRule
            {
                Id = Guid.NewGuid(),
                CollectionId = collection.Id,
                Permission = permission,
                CanRead = true,
                CanWrite = true,
                CanDelete = true,
                CanQuery = true,
                IsActive = true
            };

            _dbContext.SecurityRules.Add(rule);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionResponse(
            collection.Id,
            collection.ApplicationId,
            collection.Name,
            collection.Description,
            collection.IsActive);
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