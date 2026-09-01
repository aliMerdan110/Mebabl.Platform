using MediatR;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.GetSecurityRules;

public sealed record GetSecurityRulesQuery(
    Guid CollectionId
) : IRequest<IReadOnlyList<SecurityRuleItem>>;