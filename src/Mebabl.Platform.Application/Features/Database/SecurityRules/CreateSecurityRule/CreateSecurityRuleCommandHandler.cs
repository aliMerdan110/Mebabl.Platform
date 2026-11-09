using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.SecurityRules.CreateSecurityRule;

/// <summary>
/// Creates a draft security policy for a collection in a project owned by
/// the authenticated developer. Draft policies are inactive and fail closed.
/// </summary>
public sealed class CreateSecurityRuleCommandHandler
    : IRequestHandler<CreateSecurityRuleCommand, Guid>
{
    private const int MaximumDefinitionBytes = 64 * 1024;
    private const int MaximumJsonDepth = 16;
    private const int MaximumStatements = 100;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentDeveloper _currentDeveloper;

    public CreateSecurityRuleCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentDeveloper currentDeveloper)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentDeveloper = currentDeveloper
            ?? throw new ArgumentNullException(nameof(currentDeveloper));
    }

    public async Task<Guid> Handle(
        CreateSecurityRuleCommand request,
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

        if (request.CollectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(request.CollectionId));
        }

        var definition = CreateValidatedDefinition(request.Definition);

        try
        {
            var collectionExists = await _dbContext.Collections
                .AsNoTracking()
                .AnyAsync(
                    collection =>
                        collection.Id == request.CollectionId &&
                        collection.IsActive &&
                        !collection.IsDeleted &&
                        collection.Project.IsActive &&
                        !collection.Project.IsDeleted &&
                        collection.Project.DeveloperId == developerId,
                    cancellationToken);

            if (!collectionExists)
            {
                throw new KeyNotFoundException("Collection not found.");
            }

            var existingRule = await _dbContext.SecurityRules
                .FirstOrDefaultAsync(
                    rule => rule.CollectionId == request.CollectionId,
                    cancellationToken);

            if (existingRule is not null && !existingRule.IsDeleted)
            {
                throw new InvalidOperationException(
                    "A security policy already exists for this collection. Use the update operation.");
            }

            if (existingRule is not null)
            {
                if (existingRule.Revision == int.MaxValue)
                {
                    throw new InvalidOperationException(
                        "The security policy revision limit has been reached.");
                }

                var previousDefinition = existingRule.Definition;

                existingRule.Definition = definition;
                existingRule.Revision++;
                existingRule.IsActive = false;
                existingRule.PublishedAt = null;
                existingRule.IsDeleted = false;

                await _dbContext.SaveChangesAsync(cancellationToken);
                previousDefinition?.Dispose();
                definition = null!;
                return existingRule.Id;
            }

            var rule = new SecurityRule
            {
                Id = Guid.NewGuid(),
                CollectionId = request.CollectionId,
                Definition = definition,
                Revision = 1,
                IsActive = false,
                PublishedAt = null
            };

            _dbContext.SecurityRules.Add(rule);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Ownership has transferred to the tracked entity/context.
            definition = null!;
            return rule.Id;
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

            // An empty statements array is valid and represents deny-all.
            // The draft remains inactive until separately validated and published.
            return document;
        }
        catch
        {
            document.Dispose();
            throw;
        }
    }
}
