using System.Linq.Expressions;
using System.Text.Json;

using Mebabl.Platform.Application.Features.Database.Query;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine;

public sealed class QueryBuilder : IQueryBuilder
{
    private const int MaximumLimit = 100;
    private const int MaximumOffset = 100_000;
    private const int MaximumFilters = 20;
    private const int MaximumSorts = 5;
    private const int MaximumFieldLength = 128;
    private const int MaximumSearchLength = 200;
    private const int MaximumFilterValueLength = 256;

    private static readonly System.Reflection.MethodInfo GetPropertyMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetProperty),
            new[] { typeof(string) })!;

    private static readonly System.Reflection.MethodInfo GetStringMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetString),
            Type.EmptyTypes)!;

    private static readonly System.Reflection.MethodInfo GetDecimalMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetDecimal),
            Type.EmptyTypes)!;

    private static readonly System.Reflection.MethodInfo GetBooleanMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetBoolean),
            Type.EmptyTypes)!;

    public IQueryable<Document> Apply(
        IQueryable<Document> query,
        QueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);

        if (request.CollectionId == Guid.Empty)
            throw new ArgumentException("Collection ID is required.");

        if (request.Offset < 0 || request.Offset > MaximumOffset)
            throw new ArgumentOutOfRangeException(nameof(request.Offset));

        if (request.Limit < 1 || request.Limit > MaximumLimit)
            throw new ArgumentOutOfRangeException(
                nameof(request.Limit),
                $"Limit must be between 1 and {MaximumLimit}.");

        var filters = request.Filters ?? [];
        var sorts = request.Sorts ?? [];

        if (filters.Count > MaximumFilters)
            throw new ArgumentException("Too many filters.");

        if (sorts.Count > MaximumSorts)
            throw new ArgumentException("Too many sort fields.");

        if (request.Select is { Count: > 0 })
        {
            throw new ArgumentException(
                "Field projection is not supported yet. Omit Select.");
        }

        query = ApplyFilters(query, filters);
        query = ApplySearch(query, request.Search);

        var orderedQuery = ApplySorts(query, sorts);

        return orderedQuery
            .Skip(request.Offset)
            .Take(request.Limit);
    }

    /// <summary>
    /// Applies the continuation predicate for the default descending
    /// (CreatedAt, Id) ordering. Documents after the cursor have an older
    /// CreatedAt value, or the same timestamp and a lower stable identifier.
    /// </summary>
    public IQueryable<Document> ApplyCursor(
        IQueryable<Document> query,
        DateTime cursorCreatedAt,
        Guid cursorId)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (cursorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Cursor document ID is required.",
                nameof(cursorId));
        }

        return query.Where(document =>
            document.CreatedAt < cursorCreatedAt ||
            (document.CreatedAt == cursorCreatedAt &&
             document.Id.CompareTo(cursorId) < 0));
    }

    private static IQueryable<Document> ApplyFilters(
        IQueryable<Document> query,
        IReadOnlyList<QueryFilter> filters)
    {
        foreach (var filter in filters)
        {
            if (filter is null)
                throw new ArgumentException("A filter cannot be null.");

            var field = ValidateField(filter.Field);
            var value = ToJsonElement(filter.Value);

            if (value.ValueKind is not (
                    JsonValueKind.String or
                    JsonValueKind.Number or
                    JsonValueKind.True or
                    JsonValueKind.False or
                    JsonValueKind.Null))
            {
                throw new ArgumentException(
                    "Filters support scalar JSON values only.");
            }

            var parameter = Expression.Parameter(
                typeof(Document),
                "document");

            var data = Expression.Property(
                parameter,
                nameof(Document.Data));

            var root = Expression.Property(
                data,
                nameof(JsonDocument.RootElement));

            var jsonProperty = Expression.Call(
                root,
                GetPropertyMethod,
                Expression.Constant(field));

            var propertyExpression = BuildScalarExpression(
                jsonProperty,
                value.ValueKind);

            Expression body;

            switch (filter.Operator)
            {
                case QueryOperator.Equal:
                case QueryOperator.NotEqual:
                {
                    var constant = BuildConstant(value);

                    body = filter.Operator == QueryOperator.Equal
                        ? Expression.Equal(propertyExpression, constant)
                        : Expression.NotEqual(propertyExpression, constant);

                    break;
                }

                case QueryOperator.GreaterThan:
                case QueryOperator.GreaterThanOrEqual:
                case QueryOperator.LessThan:
                case QueryOperator.LessThanOrEqual:
                {
                    if (value.ValueKind != JsonValueKind.Number)
                    {
                        throw new ArgumentException(
                            "Comparison operators require a numeric value.");
                    }

                    var constant = BuildConstant(value);

                    body = filter.Operator switch
                    {
                        QueryOperator.GreaterThan =>
                            Expression.GreaterThan(propertyExpression, constant),

                        QueryOperator.GreaterThanOrEqual =>
                            Expression.GreaterThanOrEqual(
                                propertyExpression, constant),

                        QueryOperator.LessThan =>
                            Expression.LessThan(propertyExpression, constant),

                        QueryOperator.LessThanOrEqual =>
                            Expression.LessThanOrEqual(
                                propertyExpression, constant),

                        _ => throw new ArgumentOutOfRangeException()
                    };

                    break;
                }

                case QueryOperator.Contains:
                case QueryOperator.StartsWith:
                case QueryOperator.EndsWith:
                {
                    if (value.ValueKind != JsonValueKind.String)
                    {
                        throw new ArgumentException(
                            "String operators require a string value.");
                    }

                    var search = value.GetString();

                    if (string.IsNullOrEmpty(search) ||
                        search.Length > MaximumFilterValueLength)
                    {
                        throw new ArgumentException(
                            $"Filter string must contain 1 to {MaximumFilterValueLength} characters.");
                    }

                    var methodName = filter.Operator switch
                    {
                        QueryOperator.Contains => nameof(string.Contains),
                        QueryOperator.StartsWith => nameof(string.StartsWith),
                        QueryOperator.EndsWith => nameof(string.EndsWith),
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var method = typeof(string).GetMethod(
                        methodName,
                        new[] { typeof(string) })!;

                    body = Expression.Call(
                        propertyExpression,
                        method,
                        Expression.Constant(search));

                    break;
                }

                default:
                    throw new ArgumentException(
                        $"Unsupported filter operator: {filter.Operator}");
            }

            var predicate = Expression.Lambda<Func<Document, bool>>(
                body,
                parameter);

            query = query.Where(predicate);
        }

        return query;
    }

    private static Expression BuildScalarExpression(
        Expression jsonProperty,
        JsonValueKind valueKind)
    {
        return valueKind switch
        {
            JsonValueKind.String or JsonValueKind.Null =>
                Expression.Call(jsonProperty, GetStringMethod),

            JsonValueKind.Number =>
                Expression.Call(jsonProperty, GetDecimalMethod),

            JsonValueKind.True or JsonValueKind.False =>
                Expression.Call(jsonProperty, GetBooleanMethod),

            _ => throw new ArgumentException(
                "Unsupported JSON scalar type.")
        };
    }

    private static Expression BuildConstant(JsonElement value)
    {
        try
        {
            return value.ValueKind switch
            {
                JsonValueKind.String =>
                    Expression.Constant(
                        value.GetString(),
                        typeof(string)),

                JsonValueKind.Null =>
                    Expression.Constant(null, typeof(string)),

                JsonValueKind.Number =>
                    Expression.Constant(
                        value.GetDecimal(),
                        typeof(decimal)),

                JsonValueKind.True or JsonValueKind.False =>
                    Expression.Constant(
                        value.GetBoolean(),
                        typeof(bool)),

                _ => throw new ArgumentException(
                    "Unsupported filter value.")
            };
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(
                "The numeric filter value is invalid.",
                exception);
        }
        catch (OverflowException exception)
        {
            throw new ArgumentException(
                "The numeric filter value is outside the supported range.",
                exception);
        }
    }

    private static JsonElement ToJsonElement(object? value)
    {
        if (value is JsonElement element)
            return element.Clone();

        using var document = JsonDocument.Parse(
            JsonSerializer.Serialize(value));

        return document.RootElement.Clone();
    }

    private static string ValidateField(string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Filter field is required.");

        var normalized = field.Trim();

        if (normalized.Length > MaximumFieldLength ||
            normalized.Any(char.IsControl))
        {
            throw new ArgumentException("Filter field is invalid.");
        }

        return normalized;
    }

    private static IQueryable<Document> ApplySearch(
        IQueryable<Document> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var value = search.Trim();

        if (value.Length > MaximumSearchLength)
        {
            throw new ArgumentException(
                $"Search cannot exceed {MaximumSearchLength} characters.");
        }

        return query.Where(x => x.Key.Contains(value));
    }

    private static IOrderedQueryable<Document> ApplySorts(
        IQueryable<Document> query,
        IReadOnlyList<QuerySort> sorts)
    {
        if (sorts.Count == 0)
        {
            return query
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id);
        }

        IOrderedQueryable<Document>? ordered = null;

        var usedFields = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var sort in sorts)
        {
            if (sort is null ||
                string.IsNullOrWhiteSpace(sort.Field) ||
                string.IsNullOrWhiteSpace(sort.Direction))
            {
                throw new ArgumentException("Sort definition is invalid.");
            }

            var field = sort.Field.Trim();
            var direction = sort.Direction.Trim();

            if (!usedFields.Add(field))
                throw new ArgumentException("Duplicate sort field.");

            var descending = direction.Equals(
                "desc",
                StringComparison.OrdinalIgnoreCase);

            if (!descending &&
                !direction.Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Unsupported sort direction: {direction}");
            }

            ordered = AddSort(query, ordered, field, descending);
        }

        return ordered!.ThenBy(x => x.Id);
    }

    private static IOrderedQueryable<Document> AddSort(
        IQueryable<Document> query,
        IOrderedQueryable<Document>? ordered,
        string field,
        bool descending)
    {
        return field.ToLowerInvariant() switch
        {
            "createdat" when ordered is null =>
                descending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt),

            "createdat" =>
                descending
                    ? ordered!.ThenByDescending(x => x.CreatedAt)
                    : ordered!.ThenBy(x => x.CreatedAt),

            "updatedat" when ordered is null =>
                descending
                    ? query.OrderByDescending(x => x.UpdatedAt)
                    : query.OrderBy(x => x.UpdatedAt),

            "updatedat" =>
                descending
                    ? ordered!.ThenByDescending(x => x.UpdatedAt)
                    : ordered!.ThenBy(x => x.UpdatedAt),

            "key" when ordered is null =>
                descending
                    ? query.OrderByDescending(x => x.Key)
                    : query.OrderBy(x => x.Key),

            "key" =>
                descending
                    ? ordered!.ThenByDescending(x => x.Key)
                    : ordered!.ThenBy(x => x.Key),

            _ => throw new ArgumentException(
                $"Unsupported sort field: {field}")
        };
    }
}