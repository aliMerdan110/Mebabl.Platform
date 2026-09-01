using FluentValidation;

namespace Mebabl.Platform.Application.Features.Roles.Delete;

public sealed class DeleteRoleValidator
    : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}