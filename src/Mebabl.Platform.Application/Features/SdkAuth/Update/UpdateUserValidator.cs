using FluentValidation;

namespace Mebabl.Platform.Application.Features.SdkAuth.Update;

public sealed class UpdateUserValidator
    : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(100);
    }
}