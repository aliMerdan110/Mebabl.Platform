namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

public sealed record QueryFilter(
    string Field,
    QueryOperator Operator,
    object? Value);