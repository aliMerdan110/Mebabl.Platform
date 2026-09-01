using FluentValidation;

namespace Mebabl.Platform.Application.Features.Roles.Create;

public sealed class CreateRoleValidator
    : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);
    }
}