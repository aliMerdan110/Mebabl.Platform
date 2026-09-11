using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;

public sealed class CreateCollectionCommandValidator
    : AbstractValidator<CreateCollectionCommand>
{
    public CreateCollectionCommandValidator()
    {
       

        RuleFor(x => x.Name)
    .NotEmpty()
    .MaximumLength(100)
    .Must(x => !string.IsNullOrWhiteSpace(x))
    .WithMessage("Collection name is required.");

RuleFor(x => x.Description)
    .MaximumLength(500);
    }
}