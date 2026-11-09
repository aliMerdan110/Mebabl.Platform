using System.ComponentModel.DataAnnotations;

namespace Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

/// <summary>
/// HTTP/query input for a single collection. Project and application scope
/// are resolved from the authenticated principal, never from this request.
/// PageToken is a continuation cursor bound to the project, collection,
/// filters, and search supplied in the original query.
/// </summary>
public sealed class QueryRequest
{
    public const int DefaultPageSize = 50;
    public const int MaximumPageSize = 100;
    public const int MaximumInternalPageSize = MaximumPageSize + 1;
    public const int MaximumOffset = 100_000;
    public const int MaximumFilters = 20;
    public const int MaximumSorts = 5;
    public const int MaximumSearchLength = 200;
    public const int MaximumSelectedFields = 50;
    public const int MaximumPageTokenLength = 4096;

    [Required]
    public Guid CollectionId { get; init; }

    public IReadOnlyList<QueryFilter> Filters { get; init; } =
        Array.Empty<QueryFilter>();

    public IReadOnlyList<QuerySort> Sorts { get; init; } =
        Array.Empty<QuerySort>();

    /// <summary>Legacy offset pagination retained for compatibility.</summary>
    [Range(0, MaximumOffset)]
    public int Offset { get; init; }

    /// <summary>Allows the internal query pipeline to request one look-ahead row.</summary>
    [Range(1, MaximumInternalPageSize)]
    public int Limit { get; init; } = DefaultPageSize;

    [MaxLength(MaximumSearchLength)]
    public string? Search { get; init; }

    public IReadOnlyList<string> Select { get; init; } =
        Array.Empty<string>();

    /// <summary>
    /// Opaque continuation token returned by the previous page. Cursor
    /// pagination currently supports the default CreatedAt DESC, Id DESC sort
    /// and requires Offset to remain zero.
    /// </summary>
    [MaxLength(MaximumPageTokenLength)]
    public string? PageToken { get; init; }
}
