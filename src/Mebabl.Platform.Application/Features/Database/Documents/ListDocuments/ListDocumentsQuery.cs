using MediatR;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.ListDocuments;

/// <summary>
/// Requests the documents in a collection within the authenticated project
/// and application context. Project and application identifiers are derived
/// from the validated access token; they must never be accepted from the caller.
/// </summary>
public sealed record ListDocumentsQuery(Guid CollectionId)
    : IRequest<IReadOnlyList<DocumentResponse>>;
