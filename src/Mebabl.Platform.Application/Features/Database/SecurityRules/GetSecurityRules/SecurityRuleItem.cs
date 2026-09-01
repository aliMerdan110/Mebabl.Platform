namespace Mebabl.Platform.Application.Features.Database.SecurityRules.GetSecurityRules;

public sealed record SecurityRuleItem(
    Guid Id,
    string Permission,
    bool CanRead,
    bool CanWrite,
    bool CanDelete,
    bool CanQuery);