using MediatR;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.DeleteSecurityRule;

/// <summary>
/// Soft-deletes a collection security policy using optimistic concurrency.
/// ExpectedRevision must match the currently stored policy revision.
/// </summary>
public sealed record DeleteSecurityRuleCommand(
    Guid Id,
    int ExpectedRevision) : IRequest;
