namespace Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;

/// <summary>
/// Result of a successfully committed atomic document batch.
/// A response is returned only after the transaction has committed.
/// </summary>
public sealed record BatchWriteDocumentsResponse(
    Guid CollectionId,
    IReadOnlyList<BatchWriteDocumentResult> Results);
