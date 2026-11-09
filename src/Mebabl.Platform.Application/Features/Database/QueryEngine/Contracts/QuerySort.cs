namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

/// <summary>
/// Defines one deterministic sort term for a document query.
/// Field names are validated by the application validation pipeline and must
/// be explicitly supported by the configured query provider.
/// </summary>
public sealed record QuerySort(
    string Field,
    string Direction = QuerySort.Ascending)
{
    /// <summary>Ascending order (wire value: "asc").</summary>
    public const string Ascending = "asc";

    /// <summary>Descending order (wire value: "desc").</summary>
    public const string Descending = "desc";

    /// <summary>Creates an ascending sort term.</summary>
    public static QuerySort Asc(string field) =>
        new(field, Ascending);

    /// <summary>Creates a descending sort term.</summary>
    public static QuerySort Desc(string field) =>
        new(field, Descending);

    /// <summary>Indicates whether the wire direction is supported.</summary>
    public bool HasValidDirection =>
        string.Equals(Direction, Ascending, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(Direction, Descending, StringComparison.OrdinalIgnoreCase);
}
