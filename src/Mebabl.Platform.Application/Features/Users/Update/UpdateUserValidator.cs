using FluentValidation;

namespace Mebabl.Platform.Application.Features.Users.Update;

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