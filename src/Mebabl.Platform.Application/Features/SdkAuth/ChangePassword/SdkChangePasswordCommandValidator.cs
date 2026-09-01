using FluentValidation;

namespace Mebabl.Platform.Application.Features.SdkAuth.ChangePassword;

public sealed class SdkChangePasswordCommandValidator
    : AbstractValidator<SdkChangePasswordCommand>
{
    public SdkChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required.")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters long.");

        RuleFor(x => x)
            .Must(x => x.CurrentPassword != x.NewPassword)
            .WithMessage("New password must be different from the current password.");
    }
}