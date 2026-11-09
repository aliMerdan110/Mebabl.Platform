using System.Text.Json;
using MediatR;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

/// <summary>
/// Replaces a collection's draft security policy using optimistic concurrency.
/// ExpectedRevision must match the currently stored policy revision.
/// </summary>
public sealed record UpdateSecurityRuleCommand(
    Guid Id,
    JsonElement Definition,
    int ExpectedRevision) : IRequest;
