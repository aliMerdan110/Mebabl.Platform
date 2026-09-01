using FluentValidation;

namespace Mebabl.Platform.Application.Features.Permissions.Create;

public sealed class CreatePermissionValidator
    : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Category)
            .MaximumLength(100);
    }
}