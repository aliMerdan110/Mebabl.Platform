using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;

public sealed class UpdateCollectionCommandValidator
    : AbstractValidator<UpdateCollectionCommand>
{
    private const int MaximumNameLength = 100;
    private const int MaximumDescriptionLength = 500;

    public UpdateCollectionCommandValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty()
            .WithMessage("Collection ID is required.");

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Collection name is required.")
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("Collection name cannot contain only whitespace.")
            .MaximumLength(MaximumNameLength)
            .WithMessage($"Collection name cannot exceed {MaximumNameLength} characters.")
            .Must(value => value is null || !value.Any(char.IsControl))
            .WithMessage("Collection name cannot contain control characters.");

        RuleFor(x => x.Description)
            .MaximumLength(MaximumDescriptionLength)
            .WithMessage(
                $"Collection description cannot exceed {MaximumDescriptionLength} characters.");
    }
}
