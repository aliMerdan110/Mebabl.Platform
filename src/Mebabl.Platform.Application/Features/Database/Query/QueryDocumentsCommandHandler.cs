using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.Application.Features.Database.Query;

public sealed class QueryDocumentsCommandHandler
    : IRequestHandler<QueryDocumentsCommand, IReadOnlyList<QueryDocumentsResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;
    private readonly IQueryBuilder _queryBuilder;

    public QueryDocumentsCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security,
        IQueryBuilder queryBuilder)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _security = security;
        _queryBuilder = queryBuilder;
    }

    public async Task<IReadOnlyList<QueryDocumentsResponse>> Handle(
        QueryDocumentsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        await _security.EnsureQueryAsync(
            request.CollectionId,
            cancellationToken);

        var query = _dbContext.Documents
            .AsNoTracking()
            .Include(x => x.Collection)
            .Where(x =>
                x.CollectionId == request.CollectionId &&
                x.Collection.ApplicationId ==
                    _currentUser.ApplicationId &&
                !x.IsDeleted);

        query = _queryBuilder.Apply(
            query,
            new QueryRequest
            {
                CollectionId = request.CollectionId,
                Filters = request.Filters,
                Sorts = request.Sorts,
                Offset = request.Offset,
                Limit = request.Limit,
                Search = request.Search,
                Select = request.Select ?? []
            });

        return await query
            .Select(x => new QueryDocumentsResponse(
                x.Id,
                x.Key,
                x.Data,
                x.Version,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}