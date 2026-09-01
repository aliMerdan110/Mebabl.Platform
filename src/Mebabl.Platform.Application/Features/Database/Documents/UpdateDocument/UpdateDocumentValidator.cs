using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

public sealed class UpdateDocumentValidator
    : AbstractValidator<UpdateDocumentCommand>
{
    public UpdateDocumentValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Data)
            .NotNull();
    }
}