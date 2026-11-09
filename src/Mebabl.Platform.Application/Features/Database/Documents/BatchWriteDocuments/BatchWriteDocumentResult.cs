namespace Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;

/// <summary>
/// Result for one successfully committed operation, preserving its original
/// request index and the document version after the operation.
/// </summary>
public sealed record BatchWriteDocumentResult(
    int Index,
    BatchWriteOperationType Type,
    Guid DocumentId,
    int Version,
    bool IsDeleted);
