using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

public sealed class DeleteDocumentCommandValidator
    : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty();
    }
}