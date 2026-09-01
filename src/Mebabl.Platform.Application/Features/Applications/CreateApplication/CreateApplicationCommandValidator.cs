using FluentValidation;

namespace Mebabl.Platform.Application.Features.Applications.CreateApplication;

public sealed class CreateApplicationCommandValidator
    : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);
    }
}