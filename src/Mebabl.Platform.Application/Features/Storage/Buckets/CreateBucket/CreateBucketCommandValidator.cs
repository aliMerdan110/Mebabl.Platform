using FluentValidation;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.CreateBucket;

public sealed class CreateBucketCommandValidator
    : AbstractValidator<CreateBucketCommand>
{
    public CreateBucketCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}