using FluentValidation;

namespace Mebabl.Platform.Application.Features.Storage.Files.UploadFile;

public sealed class UploadFileCommandValidator
    : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.BucketId)
            .NotEmpty();

        RuleFor(x => x.FileName)
            .NotEmpty();

        RuleFor(x => x.ContentType)
            .NotEmpty();

        RuleFor(x => x.Length)
            .GreaterThan(0);

        RuleFor(x => x.Content)
            .NotNull();
    }
}