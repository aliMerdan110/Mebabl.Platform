namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

public sealed class QueryRequest
{
    public Guid CollectionId { get; init; }

    public List<QueryFilter> Filters { get; init; } = [];

    public List<QuerySort> Sorts { get; init; } = [];

    public int Offset { get; init; }

    public int Limit { get; init; } = 50;

    public string? Search { get; init; }

    public List<string> Select { get; init; } = [];
}