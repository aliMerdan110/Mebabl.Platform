using FluentValidation;

namespace Mebabl.Platform.Application.Features.UserRoles.RemoveRole;

public sealed class RemoveRoleFromUserValidator
    : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.RoleId)
            .NotEmpty();
    }
}