using MediatR;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;

public sealed record CreateSecurityRuleCommand(
    Guid CollectionId,
    string Permission,
    bool CanRead,
    bool CanWrite,
    bool CanDelete,
    bool CanQuery
) : IRequest<Guid>;