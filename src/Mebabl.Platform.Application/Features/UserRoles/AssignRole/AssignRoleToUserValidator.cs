using FluentValidation;

namespace Mebabl.Platform.Application.Features.UserRoles.AssignRole;

public sealed class AssignRoleToUserValidator
    : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.RoleId)
            .NotEmpty();
    }
}