

using System.Text.Json;
using Mebabl.Platform.Application.Features.Database.Query;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine;

public sealed class QueryBuilder : IQueryBuilder
{
    public IQueryable<Document> Apply(
        IQueryable<Document> query,
        QueryRequest request)
    {
        query = ApplyFilters(query, request.Filters);
        query = ApplySearch(query, request.Search);
        query = ApplySorts(query, request.Sorts);

        if (request.Offset > 0)
            query = query.Skip(request.Offset);

        var limit = request.Limit <= 0
            ? 50
            : Math.Min(request.Limit, 200);

        return query.Take(limit);
    }

    private static IQueryable<Document> ApplyFilters(
        IQueryable<Document> query,
        IReadOnlyList<QueryFilter> filters)
    {
        foreach (var filter in filters)
        {
            query = filter.Operator switch
            {
                QueryOperator.Equal =>
                    ApplyJsonFilter(
                        query,
                        filter.Field,
                        filter.Value,
                        false),

                QueryOperator.NotEqual =>
                    ApplyJsonFilter(
                        query,
                        filter.Field,
                        filter.Value,
                        true),

                QueryOperator.Contains =>
                    ApplyContainsFilter(
                        query,
                        filter.Field,
                        filter.Value),

                _ => throw new ArgumentException(
                    $"Unsupported filter operator: {filter.Operator}")
            };
        }

        return query;
    }

    private static IQueryable<Document> ApplyJsonFilter(
        IQueryable<Document> query,
        string field,
        object? value,
        bool negate)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            new Dictionary<string, object?>
            {
                [field] = value
            });

        return negate
            ? query.Where(x =>
                !x.Data.RootElement.ToString().Contains(json))
            : query.Where(x =>
                x.Data.RootElement.ToString().Contains(json));
    }

    private static IQueryable<Document> ApplyContainsFilter(
        IQueryable<Document> query,
        string field,
        object? value)
    {
        var search = value?.ToString() ?? string.Empty;

        return query.Where(x =>
            x.Data.RootElement.ToString().Contains(search));
    }

    private static IQueryable<Document> ApplySearch(
        IQueryable<Document> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var value = search.Trim();

        return query.Where(x => x.Key.Contains(value));
    }

    private static IQueryable<Document> ApplySorts(
        IQueryable<Document> query,
        IReadOnlyList<QuerySort> sorts)
    {
        foreach (var sort in sorts)
        {
            var descending = sort.Direction.Equals(
                "desc",
                StringComparison.OrdinalIgnoreCase);

            if (sort.Field.Equals(
                    "createdAt",
                    StringComparison.OrdinalIgnoreCase))
            {
                query = descending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt);
            }
            else if (sort.Field.Equals(
                         "updatedAt",
                         StringComparison.OrdinalIgnoreCase))
            {
                query = descending
                    ? query.OrderByDescending(x => x.UpdatedAt)
                    : query.OrderBy(x => x.UpdatedAt);
            }
            else if (sort.Field.Equals(
                         "key",
                         StringComparison.OrdinalIgnoreCase))
            {
                query = descending
                    ? query.OrderByDescending(x => x.Key)
                    : query.OrderBy(x => x.Key);
            }
            else
            {
                throw new ArgumentException(
                    $"Unsupported sort field: {sort.Field}");
            }
        }

        return query;
    }
}