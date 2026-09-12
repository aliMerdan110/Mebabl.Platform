using System.Text.Json.Serialization;

namespace Mebabl.Platform.Application.Features.Database.Query;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QueryOperator
{
    Equal,
    NotEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    Contains,
    StartsWith,
    EndsWith
}