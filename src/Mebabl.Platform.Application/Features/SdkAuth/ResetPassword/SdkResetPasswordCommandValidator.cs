using FluentValidation;

namespace Mebabl.Platform.Application.Features.SdkAuth.ResetPassword;

public sealed class SdkResetPasswordCommandValidator : AbstractValidator<SdkResetPasswordCommand>
{
    public SdkResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
            // يمكنك إضافة شروط إضافية لقوة كلمة المرور هنا إذا أردت (مثل الحروف الكبيرة والرموز)
    }
}