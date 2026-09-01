using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

public sealed class DeleteDocumentValidator
    : AbstractValidator<DeleteDocumentCommand>
{
    public DeleteDocumentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}