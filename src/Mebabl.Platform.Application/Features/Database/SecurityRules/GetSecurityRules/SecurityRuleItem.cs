using System.Text.Json;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.GetSecurityRules;

/// <summary>
/// Security-policy representation returned to the project Console.
/// Definition is a versioned declarative JSON policy, not executable code.
/// </summary>
public sealed record SecurityRuleItem(
    Guid Id,
    Guid CollectionId,
    JsonElement Definition,
    int Revision,
    bool IsActive,
    DateTime? PublishedAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
