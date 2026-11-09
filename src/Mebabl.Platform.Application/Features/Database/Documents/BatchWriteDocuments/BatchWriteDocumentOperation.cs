using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;

/// <summary>
/// One operation in an atomic batch. Fields are validated according to Type:
/// Create requires Key and object Data; Update requires DocumentId, Key, object
/// Data, and ExpectedVersion; Delete requires DocumentId and ExpectedVersion.
/// Identifiers and project scope are never accepted from the caller except for
/// the target DocumentId of Update/Delete.
/// </summary>
public sealed record BatchWriteDocumentOperation(
    BatchWriteOperationType Type,
    string? Key = null,
    JsonElement? Data = null,
    Guid? DocumentId = null,
    int? ExpectedVersion = null);
