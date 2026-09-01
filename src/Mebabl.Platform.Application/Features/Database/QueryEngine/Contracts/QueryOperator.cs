namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

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
    EndsWith,

    In,
    NotIn,

    Exists
}