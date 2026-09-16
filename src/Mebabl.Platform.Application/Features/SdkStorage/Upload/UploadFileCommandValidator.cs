using FluentValidation;

namespace Mebabl.Platform.Application.Features.SdkStorage.Upload;

public sealed class UploadFileCommandValidator
    : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "video/mp4",
        "video/quicktime",
        "application/pdf",
        "text/plain",
        "application/json"
    ];

    private const long MaxFileSize = 50 * 1024 * 1024;

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotNull();

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(x =>
                AllowedContentTypes.Contains(
                    x,
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                "The file content type is not allowed.");

        RuleFor(x => x.Size)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSize)
            .WithMessage(
                "The file size exceeds the allowed limit.");

        RuleFor(x => x.Path)
            .NotEmpty()
            .MaximumLength(500)
            .Must(IsValidPath)
            .WithMessage(
                "The storage path is invalid.");
    }

    private static bool IsValidPath(
        string path)
    {
        var normalized = path.Trim('/');

        if (string.IsNullOrWhiteSpace(normalized))
            return false;

        if (normalized.Contains(".."))
            return false;

        if (normalized.Contains('\\'))
            return false;

        if (normalized.Contains("//"))
            return false;

        return true;
    }
}