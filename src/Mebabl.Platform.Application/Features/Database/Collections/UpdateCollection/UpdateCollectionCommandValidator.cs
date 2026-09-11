using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;

public sealed class UpdateCollectionCommandValidator
    : AbstractValidator<UpdateCollectionCommand>
{
    public UpdateCollectionCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}