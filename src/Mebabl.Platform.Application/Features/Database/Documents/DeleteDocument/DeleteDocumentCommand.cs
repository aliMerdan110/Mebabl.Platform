using MediatR;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

public sealed record DeleteDocumentCommand(
    Guid Id
) : IRequest;