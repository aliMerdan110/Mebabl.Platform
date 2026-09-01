using FluentValidation;

namespace Mebabl.Platform.Application.Features.Developers.RefreshToken;

public sealed class RefreshDeveloperTokenValidator
    : AbstractValidator<RefreshDeveloperTokenCommand>
{
    public RefreshDeveloperTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}