using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;

public sealed class CreateDocumentCommandValidator
    : AbstractValidator<CreateDocumentCommand>
{
    public CreateDocumentCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.Key)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Data)
            .NotNull();
    }
}