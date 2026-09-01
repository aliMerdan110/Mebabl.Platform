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
    private readonly ICurrentApplication _currentApplication;
    private readonly IDocumentSecurityService _security;
    private readonly IQueryBuilder _queryBuilder;

    public QueryDocumentsCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IDocumentSecurityService security,
        IQueryBuilder queryBuilder)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
         _security = security;
         _queryBuilder = queryBuilder;
    }

   

    public async Task<IReadOnlyList<QueryDocumentsResponse>> Handle(
        QueryDocumentsCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated)
            throw new UnauthorizedAccessException();


        await _security.EnsureQueryAsync(
    request.CollectionId,
    cancellationToken);


        var query = _dbContext.Documents
            .AsNoTracking()
            .Include(x => x.Collection)
            .Where(x =>
                x.CollectionId == request.CollectionId &&
                x.Collection.ApplicationId == _currentApplication.ApplicationId &&
                !x.IsDeleted);


         var builder = _queryBuilder.Apply(
    query,
    new QueryRequest
    {
        CollectionId = request.CollectionId,
        Filters = request.Filters.ToList(),
        Sorts = request.Sorts.ToList(),
        Offset = request.Offset,
        Limit = request.Limit,
        Search = request.Search,
        Select = request.Select?.ToList() ?? []
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