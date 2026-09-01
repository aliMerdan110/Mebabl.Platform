using FluentValidation;

namespace Mebabl.Platform.Application.Features.RolePermissions.RemovePermission;

public sealed class RemovePermissionFromRoleValidator
    : AbstractValidator<RemovePermissionFromRoleCommand>
{
    public RemovePermissionFromRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.PermissionId)
            .NotEmpty();
    }
}