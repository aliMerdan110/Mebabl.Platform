using FluentValidation;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Settings;

public sealed class UpdateAuthenticationSettingsCommandValidator
    : AbstractValidator<UpdateAuthenticationSettingsCommand>
{
    public UpdateAuthenticationSettingsCommandValidator()
    {
        RuleFor(x => x.PasswordMinLength)
            .InclusiveBetween(6, 128);

        RuleFor(x => x.SessionLifetimeDays)
            .InclusiveBetween(1, 365);

        RuleFor(x => x.RefreshTokenLifetimeDays)
            .InclusiveBetween(1, 3650);

        RuleFor(x => x.MaxLoginAttempts)
            .InclusiveBetween(1, 100);

        RuleFor(x => x)
            .Must(x =>
                x.AllowPasswordAuthentication ||
                x.AllowAnonymousAuthentication)
            .WithMessage(
                "At least one authentication method must be enabled.");
    }
}