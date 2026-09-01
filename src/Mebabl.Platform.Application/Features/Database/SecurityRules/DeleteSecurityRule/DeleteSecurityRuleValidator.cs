using FluentValidation;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.DeleteSecurityRule;

public sealed class DeleteSecurityRuleValidator
    : AbstractValidator<DeleteSecurityRuleCommand>
{
    public DeleteSecurityRuleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}