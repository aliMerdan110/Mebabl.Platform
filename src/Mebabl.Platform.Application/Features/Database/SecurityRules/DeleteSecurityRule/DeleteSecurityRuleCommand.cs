using MediatR;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.DeleteSecurityRule;

public sealed record DeleteSecurityRuleCommand(
    Guid Id
) : IRequest;