using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Domain.Entities.Database;
using Mebabl.Platform.Application.Features.Database.Query;

namespace Mebabl.Platform.Infrastructure.Database.QueryEngine;

public sealed class PostgreSqlQueryBuilder : IQueryBuilder
{
    public IQueryable<Document> Apply(
        IQueryable<Document> query,
        QueryRequest request)
    {
        query = ApplyFilters(query, request);
        query = ApplyOrdering(query, request);
        query = ApplyPaging(query, request);

        return query;
    }

    private static IQueryable<Document> ApplyFilters(
        IQueryable<Document> query,
        QueryRequest request)
    {
        foreach (var filter in request.Filters)
        {
            switch (filter.Operator)
            {
                case QueryOperator.Equal:
                    query = query.Where(x =>
                        EF.Functions.JsonContains(
                            x.Data,
                            $"{{\"{filter.Field}\":\"{filter.Value}\"}}"));
                    break;

                case QueryOperator.NotEqual:
                    query = query.Where(x =>
                        !EF.Functions.JsonContains(
                            x.Data,
                            $"{{\"{filter.Field}\":\"{filter.Value}\"}}"));
                    break;
            }
        }

        return query;
    }

    private static IQueryable<Document> ApplyOrdering(
        IQueryable<Document> query,
        QueryRequest request)
    {
        if (request.Sorts.Count == 0)
            return query.OrderByDescending(x => x.CreatedAt);

        IOrderedQueryable<Document>? ordered = null;

        foreach (var sort in request.Sorts)
        {
            var descending = sort.Direction.Equals(
                "desc",
                StringComparison.OrdinalIgnoreCase);

            if (ordered is null)
            {
                ordered = sort.Field.ToLowerInvariant() switch
                {
                    "createdat" => descending
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),

                    "updatedat" => descending
                        ? query.OrderByDescending(x => x.UpdatedAt)
                        : query.OrderBy(x => x.UpdatedAt),

                    "key" => descending
                        ? query.OrderByDescending(x => x.Key)
                        : query.OrderBy(x => x.Key),

                    _ => throw new ArgumentException(
                        $"Unsupported sort field: {sort.Field}")
                };
            }
        }

        return ordered ?? query.OrderByDescending(x => x.CreatedAt);
    }

    private static IQueryable<Document> ApplyPaging(
        IQueryable<Document> query,
        QueryRequest request)
    {
        return query
            .Skip(request.Offset)
            .Take(request.Limit);
    }
}