using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Documents.GetDocument;

public sealed record GetDocumentQuery(Guid Id)
    : IRequest<GetDocumentResponse>;