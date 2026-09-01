using FluentValidation;

namespace Mebabl.Platform.Application.Features.SdkAuth.Logout;

public sealed class SdkLogoutValidator
    : AbstractValidator<SdkLogoutCommand>
{
    public SdkLogoutValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}