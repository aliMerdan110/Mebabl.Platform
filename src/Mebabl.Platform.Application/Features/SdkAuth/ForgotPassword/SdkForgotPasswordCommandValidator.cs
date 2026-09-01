using FluentValidation;

namespace Mebabl.Platform.Application.Features.SdkAuth.ForgotPassword;

public sealed class SdkForgotPasswordCommandValidator : AbstractValidator<SdkForgotPasswordCommand>
{
    public SdkForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}