using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Infrastructure.Security;

/// <summary>
/// Evaluates the published security policy for project-owned collections.
/// Access is denied by default. Explicit deny statements override allow
/// statements, and unsupported policy syntax is rejected at evaluation time.
/// </summary>
public sealed class DocumentSecurityService : IDocumentSecurityService
{
    private const int SupportedPolicyVersion = 1;
    private const int MaximumStatements = 100;

    private const string ReadOperation = "read";
    private const string WriteOperation = "write";
    private const string DeleteOperation = "delete";
    private const string QueryOperation = "query";
    private const string AuthenticatedPrincipal = "authenticated";

    private static readonly HashSet<string> SupportedOperations =
        new(StringComparer.Ordinal)
        {
            ReadOperation,
            WriteOperation,
            DeleteOperation,
            QueryOperation
        };

    private static readonly HashSet<string> SupportedRootProperties =
        new(StringComparer.Ordinal)
        {
            "version",
            "statements"
        };

    private static readonly HashSet<string> SupportedStatementProperties =
        new(StringComparer.Ordinal)
        {
            "effect",
            "operations",
            "principal"
        };

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public DocumentSecurityService(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public Task EnsureReadAsync(
        Guid collectionId,
        CancellationToken cancellationToken) =>
        EnsureOperationAsync(collectionId, ReadOperation, cancellationToken);

    public Task EnsureWriteAsync(
        Guid collectionId,
        CancellationToken cancellationToken) =>
        EnsureOperationAsync(collectionId, WriteOperation, cancellationToken);

    public Task EnsureDeleteAsync(
        Guid collectionId,
        CancellationToken cancellationToken) =>
        EnsureOperationAsync(collectionId, DeleteOperation, cancellationToken);

    public Task EnsureQueryAsync(
        Guid collectionId,
        CancellationToken cancellationToken) =>
        EnsureOperationAsync(collectionId, QueryOperation, cancellationToken);

    public async Task<IReadOnlySet<Guid>> GetReadableCollectionIdsAsync(
        IReadOnlyCollection<Guid> collectionIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(collectionIds);

        var projectId = GetAuthenticatedProjectId();

        if (collectionIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        if (collectionIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException(
                "Collection IDs cannot contain an empty identifier.",
                nameof(collectionIds));
        }

        var requestedIds = collectionIds.Distinct().ToArray();

        var policies = await GetPublishedPoliciesForProject(projectId)
            .Where(rule => requestedIds.Contains(rule.CollectionId))
            .Select(rule => new
            {
                rule.CollectionId,
                rule.Definition
            })
            .ToListAsync(cancellationToken);

        var readableIds = new HashSet<Guid>();

        foreach (var policy in policies)
        {
            try
            {
                if (IsAllowed(policy.Definition, ReadOperation))
                {
                    readableIds.Add(policy.CollectionId);
                }
            }
            finally
            {
                policy.Definition.Dispose();
            }
        }

        return readableIds;
    }

    private async Task EnsureOperationAsync(
        Guid collectionId,
        string operation,
        CancellationToken cancellationToken)
    {
        var projectId = GetAuthenticatedProjectId();

        if (collectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(collectionId));
        }

        if (!SupportedOperations.Contains(operation))
        {
            throw new ArgumentOutOfRangeException(nameof(operation));
        }

        // SecurityRuleConfiguration guarantees one current policy per
        // collection. SingleOrDefault makes any model/database inconsistency
        // explicit instead of silently selecting an arbitrary policy.
        var policy = await GetPublishedPoliciesForProject(projectId)
            .Where(rule => rule.CollectionId == collectionId)
            .Select(rule => rule.Definition)
            .SingleOrDefaultAsync(cancellationToken);

        if (policy is null)
        {
            throw new UnauthorizedAccessException(
                "No active, published security policy grants access to this collection.");
        }

        try
        {
            if (!IsAllowed(policy, operation))
            {
                throw new UnauthorizedAccessException(
                    $"The active security policy does not grant '{operation}' access to this collection.");
            }
        }
        finally
        {
            policy.Dispose();
        }
    }

    /// <summary>
    /// Returns only published policies attached to active collections and
    /// active projects inside the authenticated project boundary.
    /// Keep this predicate shared by single-collection and bulk checks.
    /// </summary>
    private IQueryable<SecurityRule> GetPublishedPoliciesForProject(Guid projectId)
    {
        return _dbContext.SecurityRules
            .AsNoTracking()
            .Where(rule =>
                rule.IsActive &&
                !rule.IsDeleted &&
                rule.PublishedAt != null &&
                rule.Collection.ProjectId == projectId &&
                rule.Collection.IsActive &&
                !rule.Collection.IsDeleted &&
                rule.Collection.Project.IsActive &&
                !rule.Collection.Project.IsDeleted);
    }

    private Guid GetAuthenticatedProjectId()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        var userId = _currentUser.UserId;
        var accountId = _currentUser.AccountId;
        var applicationId = _currentUser.ApplicationId;
        var projectId = _currentUser.ProjectId;

        if (userId == Guid.Empty ||
            accountId == Guid.Empty ||
            applicationId == Guid.Empty ||
            projectId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated project user scope is invalid.");
        }

        return projectId;
    }

    private static bool IsAllowed(
        JsonDocument? definition,
        string requestedOperation)
    {
        if (definition is null ||
            !SupportedOperations.Contains(requestedOperation))
        {
            return false;
        }

        var root = definition.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        // Reject unknown or duplicate root properties. This keeps policy
        // evaluation deterministic when the policy format evolves.
        var rootProperties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in root.EnumerateObject())
        {
            if (!SupportedRootProperties.Contains(property.Name) ||
                !rootProperties.Add(property.Name))
            {
                return false;
            }
        }

        if (!rootProperties.SetEquals(SupportedRootProperties) ||
            !root.TryGetProperty("version", out var versionElement) ||
            versionElement.ValueKind != JsonValueKind.Number ||
            !versionElement.TryGetInt32(out var version) ||
            version != SupportedPolicyVersion ||
            !root.TryGetProperty("statements", out var statementsElement) ||
            statementsElement.ValueKind != JsonValueKind.Array ||
            statementsElement.GetArrayLength() > MaximumStatements)
        {
            return false;
        }

        var hasMatchingAllow = false;

        foreach (var statement in statementsElement.EnumerateArray())
        {
            if (!TryReadStatement(
                    statement,
                    out var effect,
                    out var operations,
                    out var principal))
            {
                return false;
            }

            if (principal != AuthenticatedPrincipal ||
                !operations.Contains(requestedOperation))
            {
                continue;
            }

            // Explicit deny always wins, regardless of statement order.
            if (effect == "deny")
            {
                return false;
            }

            if (effect == "allow")
            {
                hasMatchingAllow = true;
            }
        }

        return hasMatchingAllow;
    }

    private static bool TryReadStatement(
        JsonElement statement,
        out string effect,
        out HashSet<string> operations,
        out string principal)
    {
        effect = string.Empty;
        principal = string.Empty;
        operations = new HashSet<string>(StringComparer.Ordinal);

        if (statement.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var statementProperties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in statement.EnumerateObject())
        {
            if (!SupportedStatementProperties.Contains(property.Name) ||
                !statementProperties.Add(property.Name))
            {
                return false;
            }
        }

        if (!statementProperties.SetEquals(SupportedStatementProperties) ||
            !statement.TryGetProperty("effect", out var effectElement) ||
            effectElement.ValueKind != JsonValueKind.String ||
            !statement.TryGetProperty("operations", out var operationsElement) ||
            operationsElement.ValueKind != JsonValueKind.Array ||
            !statement.TryGetProperty("principal", out var principalElement) ||
            principalElement.ValueKind != JsonValueKind.String ||
            operationsElement.GetArrayLength() == 0)
        {
            return false;
        }

        effect = effectElement.GetString()?.Trim().ToLowerInvariant() ?? string.Empty;
        principal = principalElement.GetString()?.Trim().ToLowerInvariant() ?? string.Empty;

        if (effect is not ("allow" or "deny") ||
            principal != AuthenticatedPrincipal)
        {
            return false;
        }

        foreach (var operationElement in operationsElement.EnumerateArray())
        {
            if (operationElement.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var operation = operationElement.GetString()?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(operation) ||
                !SupportedOperations.Contains(operation) ||
                !operations.Add(operation))
            {
                return false;
            }
        }

        return true;
    }
}
