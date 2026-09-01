using FluentValidation;

namespace Mebabl.Platform.Application.Features.Developers.Login;

public sealed class LoginDeveloperCommandValidator
    : AbstractValidator<LoginDeveloperCommand>
{
    public LoginDeveloperCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}