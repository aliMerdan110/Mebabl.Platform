using System.Text.Json;
using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

/// <summary>
/// Replaces a document in a project-scoped collection.
/// Project and user scope are resolved from the authenticated token and must
/// never be supplied by the client. ExpectedVersion protects against lost updates.
/// </summary>
public sealed record UpdateDocumentCommand(
    Guid CollectionId,
    Guid DocumentId,
    string Key,
    JsonDocument Data,
    int ExpectedVersion
) : IRequest;
