using FluentValidation;

namespace Mebabl.Platform.Application.Features.Developers.Register;

public sealed class RegisterDeveloperCommandValidator
    : AbstractValidator<RegisterDeveloperCommand>
{
    public RegisterDeveloperCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);
    }
}