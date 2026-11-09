namespace Mebabl.Platform.Application.Features.Database.Query;

/// <summary>
/// A bounded page of documents. Cursor pagination is preferred; NextOffset
/// is populated only for legacy offset/custom-sort requests.
/// </summary>
public sealed record QueryDocumentsPageResponse(
    Guid CollectionId,
    IReadOnlyList<QueryDocumentsResponse> Items,
    bool HasMore,
    string? NextPageToken,
    int? NextOffset);
