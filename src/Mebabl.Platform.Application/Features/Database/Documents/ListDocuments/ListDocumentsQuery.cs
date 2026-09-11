using MediatR;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.ListDocuments;

public sealed record ListDocumentsQuery(
    Guid CollectionId)
    : IRequest<IReadOnlyList<DocumentResponse>>;