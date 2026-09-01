using FluentValidation;

namespace Mebabl.Platform.Application.Features.Developers.Logout;

public sealed class LogoutDeveloperValidator
    : AbstractValidator<LogoutDeveloperCommand>
{
    public LogoutDeveloperValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}