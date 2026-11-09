using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

/// <summary>
/// Soft-deletes a document in the authenticated project's database.
/// ExpectedVersion protects the operation against stale writes.
/// </summary>
public sealed record DeleteDocumentCommand(
    Guid CollectionId,
    Guid DocumentId,
    int ExpectedVersion
) : IRequest;
