using FluentValidation;

namespace Mebabl.Platform.Application.Features.Permissions.Update;

public sealed class UpdatePermissionValidator
    : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Category)
            .MaximumLength(100);
    }
}