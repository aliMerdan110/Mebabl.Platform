using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;

public sealed class CreateSecurityRuleValidator
    : AbstractValidator<CreateSecurityRuleCommand>
{
    public CreateSecurityRuleValidator()
    {
        RuleFor(x => x.CollectionId)
            .NotEmpty();

        RuleFor(x => x.Permission)
            .NotEmpty()
            .MaximumLength(100);
    }
}