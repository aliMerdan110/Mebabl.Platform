using MediatR;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

public sealed record UpdateSecurityRuleCommand(
    Guid Id,
    string Permission,
    bool CanRead,
    bool CanWrite,
    bool CanDelete,
    bool CanQuery
) : IRequest;