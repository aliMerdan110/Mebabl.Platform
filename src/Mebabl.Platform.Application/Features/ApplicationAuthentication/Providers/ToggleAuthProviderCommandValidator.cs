using FluentValidation;

namespace Mebabl.Platform.Application.Features.ApplicationAuthentication.Providers;

public sealed class ToggleAuthProviderCommandValidator
    : AbstractValidator<ToggleAuthProviderCommand>
{
    private static readonly string[] AllowedProviders =
    [
        "email-password",
        "email-link"
    ];

    public ToggleAuthProviderCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Provider)
            .NotEmpty()
            .Must(provider =>
                AllowedProviders.Contains(
                    provider,
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage("Unsupported authentication provider.");
    }
}