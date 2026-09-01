using FluentValidation;

namespace Mebabl.Platform.Application.Features.Storage.Buckets.UpdateBucket;

public sealed class UpdateBucketCommandValidator
    : AbstractValidator<UpdateBucketCommand>
{
    public UpdateBucketCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}