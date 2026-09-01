using FluentValidation;

namespace Mebabl.Platform.Application.Features.SdkAuth.Refresh;

public sealed class SdkRefreshTokenValidator
    : AbstractValidator<SdkRefreshTokenCommand>
{
    public SdkRefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}