using FluentValidation;

namespace Mebabl.Platform.Application.Features.ApplicationPlatforms.CreateApplicationPlatform;

public sealed class CreateApplicationPlatformCommandValidator
    : AbstractValidator<CreateApplicationPlatformCommand>
{
    private static readonly string[] AllowedPlatforms =
    [
        "android",
        "ios",
        "web",
        "flutter"
    ];

    public CreateApplicationPlatformCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty();

        RuleFor(x => x.Platform)
            .NotEmpty()
            .Must(platform =>
                AllowedPlatforms.Contains(
                    platform.Trim().ToLowerInvariant()))
            .WithMessage(
                "Platform must be android, ios, web, or flutter.");

        RuleFor(x => x.Nickname)
            .MaximumLength(100);

        RuleFor(x => x.PackageName)
            .MaximumLength(255);

        RuleFor(x => x.BundleId)
            .MaximumLength(255);

        RuleFor(x => x.Domain)
            .MaximumLength(255);
    }
}