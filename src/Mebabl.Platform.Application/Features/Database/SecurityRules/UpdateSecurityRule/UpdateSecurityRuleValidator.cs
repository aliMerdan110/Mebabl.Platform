using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

public sealed class UpdateSecurityRuleValidator
    : AbstractValidator<UpdateSecurityRuleCommand>
{
    public UpdateSecurityRuleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Permission)
            .NotEmpty()
            .MaximumLength(100);
    }
}