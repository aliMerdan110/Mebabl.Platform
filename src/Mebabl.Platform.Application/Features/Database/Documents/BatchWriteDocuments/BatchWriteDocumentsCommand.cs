using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;

/// <summary>
/// Executes a bounded set of document mutations in one project collection as
/// a single atomic unit. The handler must commit all operations or roll them
/// all back. Authorization is evaluated for every operation type.
/// </summary>
public sealed record BatchWriteDocumentsCommand(
    Guid CollectionId,
    IReadOnlyList<BatchWriteDocumentOperation> Operations
) : IRequest<BatchWriteDocumentsResponse>
{
    /// <summary>Maximum number of operations accepted in one atomic batch.</summary>
    public const int MaximumOperations = 500;

    /// <summary>Maximum aggregate UTF-8 size of JSON Data values in one batch.</summary>
    public const int MaximumPayloadBytes = 1_048_576;
}
