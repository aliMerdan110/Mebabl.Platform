using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Collections.DeleteCollection;

public sealed class DeleteCollectionCommandValidator
    : AbstractValidator<DeleteCollectionCommand>
{
    public DeleteCollectionCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();
    }
}