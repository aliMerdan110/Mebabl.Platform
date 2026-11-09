using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.GetSecurityRules;

/// <summary>
/// Reads the current policy draft or published policy for a collection
/// inside a project owned by the authenticated developer.
/// Inactive policies remain visible to the Console; deleted policies do not.
/// </summary>
public sealed class GetSecurityRulesQueryHandler
    : IRequestHandler<GetSecurityRulesQuery, IReadOnlyList<SecurityRuleItem>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public GetSecurityRulesQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentDeveloper = currentDeveloper
            ?? throw new ArgumentNullException(nameof(currentDeveloper));
    }

    public async Task<IReadOnlyList<SecurityRuleItem>> Handle(
        GetSecurityRulesQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated developer is required.");
        }

        var developerId = _currentDeveloper.DeveloperId;
        if (developerId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated developer context is invalid.");
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
                    !collection.IsDeleted &&
                    !collection.Project.IsDeleted &&
                    collection.Project.DeveloperId == developerId,
                cancellationToken);

        if (!collectionExists)
        {
            throw new KeyNotFoundException("Collection not found.");
        }

        var policies = await _dbContext.SecurityRules
            .AsNoTracking()
            .Where(policy =>
                policy.CollectionId == request.CollectionId &&
                !policy.IsDeleted)
            .OrderByDescending(policy => policy.Revision)
            .ToListAsync(cancellationToken);

        return policies
            .Select(policy => new SecurityRuleItem(
                policy.Id,
                policy.CollectionId,
                policy.Definition.RootElement.Clone(),
                policy.Revision,
                policy.IsActive,
                policy.PublishedAt,
                policy.CreatedAt,
                policy.UpdatedAt))
            .ToArray();
    }
}
