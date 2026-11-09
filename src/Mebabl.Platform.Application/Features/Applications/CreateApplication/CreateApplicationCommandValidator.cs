using FluentValidation;

namespace Mebabl.Platform.Application.Features.Applications.CreateApplication;

public sealed class CreateApplicationCommandValidator
    : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9_-]+$");

        RuleFor(x => x.Description)
            .MaximumLength(2000);
    }
}