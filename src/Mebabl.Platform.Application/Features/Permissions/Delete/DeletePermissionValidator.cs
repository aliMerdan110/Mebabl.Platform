using FluentValidation;

namespace Mebabl.Platform.Application.Features.Permissions.Delete;

public sealed class DeletePermissionValidator
    : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}