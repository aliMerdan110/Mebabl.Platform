using System.Text.Json;
using MediatR;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;

/// <summary>
/// Creates a draft, versioned security policy for a collection.
/// The policy is not activated until it has been validated and published.
/// </summary>
public sealed record CreateSecurityRuleCommand(
    Guid CollectionId,
    JsonElement Definition) : IRequest<Guid>;
