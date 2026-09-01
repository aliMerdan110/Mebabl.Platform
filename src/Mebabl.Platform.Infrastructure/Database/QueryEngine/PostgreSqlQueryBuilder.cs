using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Domain.Entities.Database;

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

                // case QueryOperator.Exists:

                //     query = query.Where(x =>
                //         x.Data.RootElement.TryGetProperty(
                //             filter.Field,
                //             out _));

                //     break;
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
            ordered ??=
                sort.Descending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt);
        }

        return ordered ?? query;
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