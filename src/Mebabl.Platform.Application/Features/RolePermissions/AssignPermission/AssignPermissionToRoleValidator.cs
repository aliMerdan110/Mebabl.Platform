using FluentValidation;

namespace Mebabl.Platform.Application.Features.RolePermissions.AssignPermission;

public sealed class AssignPermissionToRoleValidator
    : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.PermissionId)
            .NotEmpty();
    }
}