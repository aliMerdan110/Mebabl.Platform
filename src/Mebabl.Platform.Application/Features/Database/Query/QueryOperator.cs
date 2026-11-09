using System.Text.Json.Serialization;

namespace Mebabl.Platform.Application.Features.Database.Query;

/// <summary>
/// Supported operators for querying JSON document fields.
/// The API serializes operators by their stable string names.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QueryOperator
{
    // Scalar equality and comparison.
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    // String matching. These operate on JSON string fields.
    Contains,
    StartsWith,
    EndsWith,

    // Set membership. The filter value must be a JSON array of scalar values.
    In,
    NotIn,

    // JSON array matching. The target field must be a JSON array.
    ArrayContains,
    ArrayContainsAny,
    ArrayContainsAll,

    // Field existence and null checks.
    IsNull,
    IsNotNull,
    Exists,
    NotExists
}
