using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

public sealed class DeleteDocumentCommandValidator
    : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.DocumentId)
            .NotEmpty();

        RuleFor(x => x.ExpectedVersion)
            .GreaterThan(0);
    }
}
