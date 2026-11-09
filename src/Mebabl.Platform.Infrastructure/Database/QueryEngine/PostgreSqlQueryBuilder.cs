using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Features.Database.Query;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Infrastructure.Database.QueryEngine;

/// <summary>
/// Builds server-side PostgreSQL queries over JSONB documents.
/// Dynamic field names and filter values are expression constants and are
/// translated as parameters by EF Core/Npgsql; they are never interpolated
/// into SQL text.
/// </summary>
public sealed class PostgreSqlQueryBuilder : IQueryBuilder
{
    private static readonly MethodInfo GetPropertyMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetProperty),
            [typeof(string)])
        ?? throw new InvalidOperationException(
            "JsonElement.GetProperty(string) was not found.");

    private static readonly MethodInfo GetStringMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetString),
            Type.EmptyTypes)
        ?? throw new InvalidOperationException(
            "JsonElement.GetString() was not found.");

    private static readonly MethodInfo GetDecimalMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetDecimal),
            Type.EmptyTypes)
        ?? throw new InvalidOperationException(
            "JsonElement.GetDecimal() was not found.");

    private static readonly MethodInfo GetBooleanMethod =
        typeof(JsonElement).GetMethod(
            nameof(JsonElement.GetBoolean),
            Type.EmptyTypes)
        ?? throw new InvalidOperationException(
            "JsonElement.GetBoolean() was not found.");

    private static readonly PropertyInfo EfFunctionsProperty =
        typeof(EF).GetProperty(
            nameof(EF.Functions),
            BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException(
            "EF.Functions was not found.");

    private static readonly MethodInfo JsonTypeofMethod =
        typeof(NpgsqlJsonDbFunctionsExtensions).GetMethod(
            nameof(NpgsqlJsonDbFunctionsExtensions.JsonTypeof),
            [typeof(DbFunctions), typeof(object)])
        ?? throw new InvalidOperationException(
            "Npgsql JSONB type inspection function was not found.");

    public IQueryable<Document> ApplyCursor(
        IQueryable<Document> query,
        DateTime createdAtUtc,
        Guid documentId)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (documentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Cursor document ID is required.",
                nameof(documentId));
        }

        // Npgsql translates this row-value comparison to:
        // ("CreatedAt", "Id") < (@createdAtUtc, @documentId).
        // Keeping the provider-specific expression here prevents the
        // Application layer from depending on Npgsql APIs.
        return query.Where(document =>
            EF.Functions.LessThan(
                ValueTuple.Create(document.CreatedAt, document.Id),
                ValueTuple.Create(createdAtUtc, documentId)));
    }

    public IQueryable<Document> Apply(
        IQueryable<Document> query,
        QueryRequest request)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);

        if (request.CollectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(request.CollectionId));
        }

        ValidatePaging(request.Offset, request.Limit);

        var filters = request.Filters ?? Array.Empty<QueryFilter>();
        var sorts = request.Sorts ?? Array.Empty<QuerySort>();
        var selectedFields = request.Select ?? Array.Empty<string>();

        if (filters.Count > QueryRequest.MaximumFilters)
        {
            throw new ArgumentException(
                $"A query cannot contain more than {QueryRequest.MaximumFilters} filters.",
                nameof(request.Filters));
        }

        if (sorts.Count > QueryRequest.MaximumSorts)
        {
            throw new ArgumentException(
                $"A query cannot contain more than {QueryRequest.MaximumSorts} sort fields.",
                nameof(request.Sorts));
        }

        // QueryDocumentsResponse currently returns complete documents. Reject
        // projection rather than silently returning fields the caller omitted.
        if (selectedFields.Count > 0)
        {
            throw new ArgumentException(
                "Field projection is not supported by this query result contract.",
                nameof(request.Select));
        }

        query = ApplyFilters(query, filters);
        query = ApplySearch(query, request.Search);

        var orderedQuery = ApplySorts(query, sorts);

        return orderedQuery
            .Skip(request.Offset)
            .Take(request.Limit);
    }

    private static void ValidatePaging(int offset, int limit)
    {
        if (offset < 0 || offset > QueryRequest.MaximumOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                $"Offset must be between 0 and {QueryRequest.MaximumOffset}.");
        }

        if (limit < 1 || limit > QueryRequest.MaximumInternalPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                $"Limit must be between 1 and {QueryRequest.MaximumInternalPageSize}.");
        }
    }

    private static IQueryable<Document> ApplyFilters(
        IQueryable<Document> query,
        IReadOnlyList<QueryFilter> filters)
    {
        foreach (var filter in filters)
        {
            if (filter is null)
            {
                throw new ArgumentException(
                    "A query filter cannot be null.",
                    nameof(filters));
            }

            var fieldPath = ValidateFieldPath(filter.Field);
            var value = ToJsonElement(filter.Value);

            if (value.ValueKind is not (
                JsonValueKind.String or
                JsonValueKind.Number or
                JsonValueKind.True or
                JsonValueKind.False or
                JsonValueKind.Null))
            {
                throw new ArgumentException(
                    "Query filters support scalar JSON values only.",
                    nameof(filter));
            }

            var parameter = Expression.Parameter(typeof(Document), "document");
            var data = Expression.Property(parameter, nameof(Document.Data));
            var root = Expression.Property(data, nameof(JsonDocument.RootElement));
            var jsonProperty = BuildJsonPropertyExpression(root, fieldPath);
            var jsonType = BuildJsonTypeExpression(jsonProperty);

            Expression predicateBody;

            if (value.ValueKind == JsonValueKind.Null)
            {
                // JSON null is distinct from a missing path. jsonb_typeof on a
                // missing path returns SQL NULL, so both comparisons exclude it.
                var isJsonNull = Expression.Equal(
                    jsonType,
                    Expression.Constant("null"));

                predicateBody = filter.Operator switch
                {
                    QueryOperator.Equal => isJsonNull,
                    QueryOperator.NotEqual => Expression.Not(isJsonNull),
                    _ => throw new ArgumentException(
                        "JSON null supports only Equal and NotEqual operators.",
                        nameof(filter.Operator))
                };
            }
            else
            {
                var expectedJsonType = GetJsonTypeName(value.ValueKind);
                var hasExpectedType = Expression.Equal(
                    jsonType,
                    Expression.Constant(expectedJsonType));

                // Npgsql translates JsonElement.Get* calls into JSON traversal
                // and PostgreSQL casts. The CASE expression prevents invalid
                // casts when stored data has a different JSON scalar type.
                var scalar = BuildScalarExpression(jsonProperty, value.ValueKind);
                var safeScalar = Expression.Condition(
                    hasExpectedType,
                    scalar,
                    Expression.Default(scalar.Type));

                var comparison = BuildComparison(
                    safeScalar,
                    value,
                    filter.Operator);

                // Keep the type predicate separate from the CASE. This makes
                // the type semantics explicit and prevents a missing field's
                // fallback value from matching (for example) numeric zero.
                predicateBody = Expression.AndAlso(
                    hasExpectedType,
                    comparison);
            }

            var predicate = Expression.Lambda<Func<Document, bool>>(
                predicateBody,
                parameter);

            // Multiple filters are combined with AND.
            query = query.Where(predicate);
        }

        return query;
    }

    private static Expression BuildJsonPropertyExpression(
        Expression root,
        IReadOnlyList<string> fieldPath)
    {
        var current = root;

        foreach (var segment in fieldPath)
        {
            current = Expression.Call(
                current,
                GetPropertyMethod,
                Expression.Constant(segment));
        }

        return current;
    }

    private static Expression BuildJsonTypeExpression(Expression jsonValue)
    {
        var functions = Expression.Property(null, EfFunctionsProperty);
        var boxedJsonValue = Expression.Convert(jsonValue, typeof(object));

        return Expression.Call(
            JsonTypeofMethod,
            functions,
            boxedJsonValue);
    }

    private static Expression BuildScalarExpression(
        Expression jsonProperty,
        JsonValueKind valueKind)
    {
        return valueKind switch
        {
            JsonValueKind.String =>
                Expression.Call(jsonProperty, GetStringMethod),

            JsonValueKind.Number =>
                Expression.Call(jsonProperty, GetDecimalMethod),

            JsonValueKind.True or JsonValueKind.False =>
                Expression.Call(jsonProperty, GetBooleanMethod),

            _ => throw new ArgumentException(
                "The JSON value is not a supported non-null scalar.",
                nameof(valueKind))
        };
    }

    private static Expression BuildComparison(
        Expression property,
        JsonElement value,
        QueryOperator queryOperator)
    {
        return queryOperator switch
        {
            QueryOperator.Equal =>
                Expression.Equal(property, BuildConstant(value)),

            QueryOperator.NotEqual =>
                Expression.NotEqual(property, BuildConstant(value)),

            QueryOperator.GreaterThan => BuildNumericComparison(
                property, value, Expression.GreaterThan),

            QueryOperator.GreaterThanOrEqual => BuildNumericComparison(
                property, value, Expression.GreaterThanOrEqual),

            QueryOperator.LessThan => BuildNumericComparison(
                property, value, Expression.LessThan),

            QueryOperator.LessThanOrEqual => BuildNumericComparison(
                property, value, Expression.LessThanOrEqual),

            QueryOperator.Contains => BuildStringPredicate(
                property, value, nameof(string.Contains)),

            QueryOperator.StartsWith => BuildStringPredicate(
                property, value, nameof(string.StartsWith)),

            QueryOperator.EndsWith => BuildStringPredicate(
                property, value, nameof(string.EndsWith)),

            _ => throw new ArgumentOutOfRangeException(
                nameof(queryOperator),
                queryOperator,
                "Unsupported query operator.")
        };
    }

    private static Expression BuildConstant(JsonElement value)
    {
        try
        {
            return value.ValueKind switch
            {
                JsonValueKind.String => Expression.Constant(
                    value.GetString(),
                    typeof(string)),

                JsonValueKind.Number => Expression.Constant(
                    value.GetDecimal(),
                    typeof(decimal)),

                JsonValueKind.True or JsonValueKind.False => Expression.Constant(
                    value.GetBoolean(),
                    typeof(bool)),

                _ => throw new ArgumentException(
                    "Unsupported filter value.",
                    nameof(value))
            };
        }
        catch (FormatException exception)
        {
            throw new ArgumentException(
                "The numeric filter value is invalid.",
                nameof(value),
                exception);
        }
        catch (OverflowException exception)
        {
            throw new ArgumentException(
                "The numeric filter value is outside the supported decimal range.",
                nameof(value),
                exception);
        }
    }

    private static Expression BuildNumericComparison(
        Expression property,
        JsonElement value,
        Func<Expression, Expression, BinaryExpression> comparison)
    {
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new ArgumentException(
                "Comparison operators require a numeric filter value.",
                nameof(value));
        }

        return comparison(property, BuildConstant(value));
    }

    private static Expression BuildStringPredicate(
        Expression property,
        JsonElement value,
        string methodName)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new ArgumentException(
                "String operators require a string filter value.",
                nameof(value));
        }

        var search = value.GetString();
        if (string.IsNullOrEmpty(search) || search.Length > 256)
        {
            throw new ArgumentException(
                "String filter values must contain 1 to 256 characters.",
                nameof(value));
        }

        var method = typeof(string).GetMethod(
            methodName,
            [typeof(string)])
            ?? throw new InvalidOperationException(
                $"String method '{methodName}' was not found.");

        return Expression.Call(
            property,
            method,
            Expression.Constant(search));
    }

    private static IQueryable<Document> ApplySearch(
        IQueryable<Document> query,
        string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var normalized = search.Trim();
        if (normalized.Length > QueryRequest.MaximumSearchLength ||
            normalized.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"Search must not exceed {QueryRequest.MaximumSearchLength} characters and cannot contain control characters.",
                nameof(search));
        }

        // Search currently targets the stable document key. Full-text search
        // over JSON fields requires an explicit index/configuration contract.
        return query.Where(document => document.Key.Contains(normalized));
    }

    private static IOrderedQueryable<Document> ApplySorts(
        IQueryable<Document> query,
        IReadOnlyList<QuerySort> sorts)
    {
        if (sorts.Count == 0)
        {
            return query
                .OrderByDescending(document => document.CreatedAt)
                .ThenByDescending(document => document.Id);
        }

        IOrderedQueryable<Document>? ordered = null;
        var usedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var sort in sorts)
        {
            if (sort is null)
            {
                throw new ArgumentException(
                    "A sort definition cannot be null.",
                    nameof(sorts));
            }

            var field = sort.Field?.Trim();
            var direction = sort.Direction?.Trim();

            if (string.IsNullOrWhiteSpace(field) ||
                string.IsNullOrWhiteSpace(direction))
            {
                throw new ArgumentException(
                    "Sort field and direction are required.",
                    nameof(sorts));
            }

            if (!usedFields.Add(field))
            {
                throw new ArgumentException(
                    $"Duplicate sort field '{field}'.",
                    nameof(sorts));
            }

            var descending = direction.Equals(
                QuerySort.Descending,
                StringComparison.OrdinalIgnoreCase);

            if (!descending && !direction.Equals(
                QuerySort.Ascending,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    $"Unsupported sort direction '{direction}'. Use 'asc' or 'desc'.",
                    nameof(sorts));
            }

            ordered = AddSort(query, ordered, field, descending);
        }

        // Append a unique key so paging remains deterministic when sort values tie.
        return ordered!.ThenBy(document => document.Id);
    }

    private static IOrderedQueryable<Document> AddSort(
        IQueryable<Document> query,
        IOrderedQueryable<Document>? ordered,
        string field,
        bool descending)
    {
        return field.ToLowerInvariant() switch
        {
            "createdat" when ordered is null => descending
                ? query.OrderByDescending(document => document.CreatedAt)
                : query.OrderBy(document => document.CreatedAt),

            "createdat" => descending
                ? ordered!.ThenByDescending(document => document.CreatedAt)
                : ordered!.ThenBy(document => document.CreatedAt),

            "updatedat" when ordered is null => descending
                ? query.OrderByDescending(document => document.UpdatedAt)
                : query.OrderBy(document => document.UpdatedAt),

            "updatedat" => descending
                ? ordered!.ThenByDescending(document => document.UpdatedAt)
                : ordered!.ThenBy(document => document.UpdatedAt),

            "key" when ordered is null => descending
                ? query.OrderByDescending(document => document.Key)
                : query.OrderBy(document => document.Key),

            "key" => descending
                ? ordered!.ThenByDescending(document => document.Key)
                : ordered!.ThenBy(document => document.Key),

            "version" when ordered is null => descending
                ? query.OrderByDescending(document => document.Version)
                : query.OrderBy(document => document.Version),

            "version" => descending
                ? ordered!.ThenByDescending(document => document.Version)
                : ordered!.ThenBy(document => document.Version),

            _ => throw new ArgumentException(
                $"Unsupported sort field '{field}'. Supported fields: createdAt, updatedAt, key, version.",
                nameof(field))
        };
    }

    private static string[] ValidateFieldPath(string? field)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            throw new ArgumentException(
                "Filter field is required.",
                nameof(field));
        }

        var normalized = field.Trim();
        if (normalized.Length > 128 || normalized.Any(char.IsControl))
        {
            throw new ArgumentException(
                "Filter field path is invalid.",
                nameof(field));
        }

        var segments = normalized.Split('.', StringSplitOptions.None);
        if (segments.Length is < 1 or > 8 ||
            segments.Any(segment =>
                segment.Length is < 1 or > 64 ||
                !IsValidFieldSegment(segment)))
        {
            throw new ArgumentException(
                "Filter field paths must contain 1 to 8 valid JSON property segments.",
                nameof(field));
        }

        return segments;
    }

    private static bool IsValidFieldSegment(string segment)
    {
        if (!(char.IsLetter(segment[0]) || segment[0] == '_'))
        {
            return false;
        }

        return segment.All(character =>
            char.IsLetterOrDigit(character) ||
            character == '_' ||
            character == '-');
    }

    private static string GetJsonTypeName(JsonValueKind valueKind)
    {
        return valueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => "number",
            JsonValueKind.True or JsonValueKind.False => "boolean",
            JsonValueKind.Null => "null",
            _ => throw new ArgumentOutOfRangeException(nameof(valueKind))
        };
    }

    private static JsonElement ToJsonElement(object? value)
    {
        return value switch
        {
            JsonElement element => element.Clone(),
            JsonDocument document => document.RootElement.Clone(),
            _ => JsonSerializer.SerializeToElement(value)
        };
    }
}
