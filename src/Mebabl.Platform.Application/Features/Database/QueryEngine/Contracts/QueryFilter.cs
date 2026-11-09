using System.Text.Json;
using System.Text.Json.Serialization;

using Mebabl.Platform.Application.Features.Database.Query;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

/// <summary>
/// A strongly-typed, JSON-safe filter definition sent to the database query
/// engine. Keeping the value as <see cref="JsonElement"/> preserves its JSON
/// scalar type during HTTP deserialization (string, number, boolean, or null)
/// instead of relying on the ambiguous runtime type of <see cref="object"/>.
/// </summary>
public sealed record QueryFilter
{
    [JsonConstructor]
    public QueryFilter(
        string field,
        QueryOperator @operator,
        JsonElement value)
    {
        if (string.IsNullOrWhiteSpace(field))
        {
            throw new ArgumentException(
                "Filter field is required.",
                nameof(field));
        }

        if (value.ValueKind == JsonValueKind.Undefined)
        {
            throw new ArgumentException(
                "Filter value is required.",
                nameof(value));
        }

        Field = field.Trim();
        Operator = @operator;
        Value = value.Clone();
    }

    /// <summary>
    /// Convenience overload for application code that builds queries without
    /// first creating a JsonElement. Values are serialized immediately so the
    /// stored filter has the same representation as an HTTP query payload.
    /// </summary>
    public QueryFilter(
        string field,
        QueryOperator @operator,
        object? value)
        : this(
            field,
            @operator,
            JsonSerializer.SerializeToElement(
                value,
                value?.GetType() ?? typeof(object)))
    {
    }

    public string Field { get; init; }

    public QueryOperator Operator { get; init; }

    public JsonElement Value { get; init; }
}
