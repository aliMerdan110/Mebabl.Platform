using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;

public sealed class CreateCollectionCommandValidator
    : AbstractValidator<CreateCollectionCommand>
{
    private const int MaximumNameLength = 100;
    private const int MaximumDescriptionLength = 500;

    public CreateCollectionCommandValidator()
    {
        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Collection name is required.")
            .Must(name => name is null || !name.Any(char.IsControl))
            .WithMessage("Collection name cannot contain control characters.")
            .MaximumLength(MaximumNameLength);

        RuleFor(command => command.Description)
            .Must(description =>
                description is null || description.Trim().Length <= MaximumDescriptionLength)
            .WithMessage(
                $"Collection description cannot exceed {MaximumDescriptionLength} characters.");
    }
}
