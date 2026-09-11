using MediatR;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.GetDocument;

public sealed record GetDocumentQuery(
    Guid DocumentId)
    : IRequest<DocumentResponse>;