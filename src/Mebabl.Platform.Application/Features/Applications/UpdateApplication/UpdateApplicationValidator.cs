using FluentValidation;

namespace Mebabl.Platform.Application.Features.Applications.UpdateApplication;

public sealed class UpdateApplicationValidator
    : AbstractValidator<UpdateApplicationCommand>
{
    public UpdateApplicationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Domain)
            .MaximumLength(500);
    }
}