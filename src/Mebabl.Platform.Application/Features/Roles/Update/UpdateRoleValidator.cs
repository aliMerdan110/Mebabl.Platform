using FluentValidation;

namespace Mebabl.Platform.Application.Features.Roles.Update;

public sealed class UpdateRoleValidator
    : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);
    }
}