
using FluentValidation;

namespace Mebabl.Platform.Application.Features.Projects.Platforms.CreateProjectPlatform;

public sealed class CreateProjectPlatformCommandValidator
    : AbstractValidator<CreateProjectPlatformCommand>
{
    private static readonly string[] SupportedPlatforms =
    [
        "android",
        "ios",
        "web"
    ];

    public CreateProjectPlatformCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Platform)
            .NotEmpty()
            .Must(platform =>
                SupportedPlatforms.Contains(
                    platform.Trim().ToLowerInvariant()))
            .WithMessage("Platform must be android, ios, or web.");

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
