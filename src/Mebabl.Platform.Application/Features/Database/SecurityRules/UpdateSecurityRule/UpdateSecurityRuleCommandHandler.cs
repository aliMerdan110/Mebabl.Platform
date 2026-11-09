using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.UpdateSecurityRule;

/// <summary>
/// Replaces a collection policy owned by the authenticated developer.
/// Every update invalidates the published revision until it is validated
/// and explicitly published again.
/// </summary>
public sealed class UpdateSecurityRuleCommandHandler
    : IRequestHandler<UpdateSecurityRuleCommand>
{
    private const int MaximumDefinitionBytes = 64 * 1024;
    private const int MaximumJsonDepth = 16;
    private const int MaximumStatements = 100;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public UpdateSecurityRuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentDeveloper = currentDeveloper
            ?? throw new ArgumentNullException(nameof(currentDeveloper));
    }

    public async Task Handle(
        UpdateSecurityRuleCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentDeveloper.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated developer is required.");
        }

        var developerId = _currentDeveloper.DeveloperId;
        if (developerId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated developer context is invalid.");
        }

        if (request.Id == Guid.Empty)
        {
            throw new ArgumentException(
                "Security policy ID is required.",
                nameof(request.Id));
        }

        if (request.ExpectedRevision < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.ExpectedRevision),
                "Expected revision must be a positive number.");
        }

        var definition = CreateValidatedDefinition(request.Definition);
        try
        {
            var rule = await _dbContext.SecurityRules
                .FirstOrDefaultAsync(
                    policy =>
                        policy.Id == request.Id &&
                        !policy.IsDeleted &&
                        policy.Collection.IsActive &&
                        !policy.Collection.IsDeleted &&
                        policy.Collection.Project.IsActive &&
                        !policy.Collection.Project.IsDeleted &&
                        policy.Collection.Project.DeveloperId == developerId,
                    cancellationToken);

            if (rule is null)
            {
                throw new KeyNotFoundException("Security policy not found.");
            }

            if (rule.Revision != request.ExpectedRevision)
            {
                throw new InvalidOperationException(
                    "Security policy revision conflict. Reload the policy before updating it.");
            }

            if (rule.Revision == int.MaxValue)
            {
                throw new InvalidOperationException(
                    "The security policy revision limit has been reached.");
            }

            var previousDefinition = rule.Definition;

            rule.Definition = definition;
            rule.Revision++;
            rule.IsActive = false;
            rule.PublishedAt = null;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new InvalidOperationException(
                    "Security policy was modified by another request. Reload it and try again.",
                    exception);
            }

            previousDefinition?.Dispose();
            definition = null!;
        }
        finally
        {
            definition?.Dispose();
        }
    }

    private static JsonDocument CreateValidatedDefinition(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException(
                "Security policy definition must be a JSON object.",
                nameof(value));
        }

        var rawJson = value.GetRawText();
        if (Encoding.UTF8.GetByteCount(rawJson) > MaximumDefinitionBytes)
        {
            throw new ArgumentException(
                $"Security policy definition cannot exceed {MaximumDefinitionBytes} bytes.",
                nameof(value));
        }

        var document = JsonDocument.Parse(
            rawJson,
            new JsonDocumentOptions
            {
                MaxDepth = MaximumJsonDepth,
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

        try
        {
            var root = document.RootElement;
            if (!root.TryGetProperty("version", out var version) ||
                version.ValueKind != JsonValueKind.Number ||
                !version.TryGetInt32(out var versionNumber) ||
                versionNumber != 1)
            {
                throw new ArgumentException(
                    "Security policy definition must declare version 1.",
                    nameof(value));
            }

            if (!root.TryGetProperty("statements", out var statements) ||
                statements.ValueKind != JsonValueKind.Array ||
                statements.GetArrayLength() > MaximumStatements)
            {
                throw new ArgumentException(
                    $"Security policy definition must contain a statements array with no more than {MaximumStatements} entries.",
                    nameof(value));
            }

            foreach (var statement in statements.EnumerateArray())
            {
                if (statement.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException(
                        "Every security policy statement must be a JSON object.",
                        nameof(value));
                }
            }

            return document;
        }
        catch
        {
            document.Dispose();
            throw;
        }
    }
}
