using System.Security.Cryptography;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.QueryEngine.Contracts;

namespace Mebabl.Platform.Application.Features.Database.Query;

/// <summary>
/// Executes queries against project-owned documents. The default query path
/// uses keyset pagination over (CreatedAt DESC, Id DESC), matching the
/// project/collection chronological index. Custom sorts and non-zero offsets
/// remain available through the legacy offset path during SDK migration.
/// </summary>
public sealed class QueryDocumentsCommandHandler
    : IRequestHandler<QueryDocumentsCommand, QueryDocumentsPageResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;
    private readonly IQueryBuilder _queryBuilder;

    public QueryDocumentsCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security,
        IQueryBuilder queryBuilder)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _security = security
            ?? throw new ArgumentNullException(nameof(security));
        _queryBuilder = queryBuilder
            ?? throw new ArgumentNullException(nameof(queryBuilder));
    }

    public async Task<QueryDocumentsPageResponse> Handle(
        QueryDocumentsCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated application user is required.");
        }

        var projectId = _currentUser.ProjectId;
        var applicationId = _currentUser.ApplicationId;
        var userId = _currentUser.UserId;
        var accountId = _currentUser.AccountId;

        if (projectId == Guid.Empty ||
            applicationId == Guid.Empty ||
            userId == Guid.Empty ||
            accountId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated project user scope is invalid.");
        }

        ValidateRequest(request);

        var hasCustomSort = request.Sorts is { Count: > 0 };
        var hasPageToken = request.PageToken is not null;
        var useCursor = !hasCustomSort && request.Offset == 0;

        if (hasPageToken && !useCursor)
        {
            throw new ArgumentException(
                "PageToken can be used only with the default sort and Offset=0.",
                nameof(request.PageToken));
        }

        if (hasPageToken && string.IsNullOrWhiteSpace(request.PageToken))
        {
            throw new ArgumentException(
                "PageToken cannot be empty.",
                nameof(request.PageToken));
        }

        var queryFingerprint = CreateQueryFingerprint(request);
        QueryCursorPosition? cursor = null;

        if (hasPageToken)
        {
            cursor = QueryCursorTokenCodec.Decode(
                request.PageToken!,
                projectId,
                request.CollectionId,
                queryFingerprint);
        }

        var collectionExists = await _dbContext.Collections
            .AsNoTracking()
            .AnyAsync(
                collection =>
                    collection.Id == request.CollectionId &&
                    collection.ProjectId == projectId &&
                    collection.IsActive &&
                    !collection.IsDeleted &&
                    collection.Project.IsActive &&
                    !collection.Project.IsDeleted,
                cancellationToken);

        if (!collectionExists)
        {
            throw new KeyNotFoundException("Collection not found.");
        }

        await _security.EnsureReadAsync(request.CollectionId, cancellationToken);
        await _security.EnsureQueryAsync(request.CollectionId, cancellationToken);

        var query = _dbContext.Documents
            .AsNoTracking()
            .Where(document =>
                document.ProjectId == projectId &&
                document.CollectionId == request.CollectionId &&
                !document.IsDeleted &&
                document.Collection.ProjectId == projectId &&
                document.Collection.IsActive &&
                !document.Collection.IsDeleted &&
                document.Project.IsActive &&
                !document.Project.IsDeleted);

        if (cursor.HasValue)
        {
            var position = cursor.Value;
            query = _queryBuilder.ApplyCursor(
                query,
                position.CreatedAtUtc,
                position.DocumentId);
        }

        var queryRequest = new QueryRequest
        {
            CollectionId = request.CollectionId,
            Filters = request.Filters ?? Array.Empty<QueryFilter>(),
            Sorts = request.Sorts ?? Array.Empty<QuerySort>(),
            Offset = useCursor ? 0 : request.Offset,
            // One extra row tells the caller whether a next page exists.
            Limit = request.Limit + 1,
            Search = request.Search,
            Select = request.Select ?? Array.Empty<string>()
        };

        var queryResults = await _queryBuilder
            .Apply(query, queryRequest)
            .Select(document => new QueryDocumentsResponse(
                document.Id,
                document.Key,
                document.Data,
                document.Version,
                document.CreatedAt,
                document.UpdatedAt))
            .ToListAsync(cancellationToken);

        var hasMore = queryResults.Count > request.Limit;
        if (hasMore)
        {
            queryResults.RemoveAt(queryResults.Count - 1);
        }

        string? nextPageToken = null;
        int? nextOffset = null;

        if (hasMore && useCursor && queryResults.Count > 0)
        {
            var lastDocument = queryResults[^1];
            nextPageToken = QueryCursorTokenCodec.Encode(
                projectId,
                request.CollectionId,
                lastDocument.CreatedAt,
                lastDocument.Id,
                queryFingerprint);
        }
        else if (hasMore)
        {
            nextOffset = checked(request.Offset + request.Limit);
        }

        return new QueryDocumentsPageResponse(
            request.CollectionId,
            queryResults,
            hasMore,
            nextPageToken,
            nextOffset);
    }

    private static void ValidateRequest(QueryDocumentsCommand request)
    {
        if (request.CollectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(request.CollectionId));
        }

        if (request.Offset < 0 || request.Offset > QueryRequest.MaximumOffset)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Offset),
                $"Offset must be between 0 and {QueryRequest.MaximumOffset}.");
        }

        if (request.Limit < 1 || request.Limit > QueryRequest.MaximumPageSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.Limit),
                $"Limit must be between 1 and {QueryRequest.MaximumPageSize}.");
        }

        if (request.Filters is { Count: > QueryRequest.MaximumFilters } ||
            request.Sorts is { Count: > QueryRequest.MaximumSorts })
        {
            throw new ArgumentException(
                "The query exceeds the supported filter or sort limits.");
        }

        if (request.Select is { Count: > 0 })
        {
            throw new ArgumentException(
                "Field projection is not supported by this query result contract.",
                nameof(request.Select));
        }

        if (request.PageToken is { Length: > QueryDocumentsCommand.MaximumPageTokenLength })
        {
            throw new ArgumentException(
                $"PageToken cannot exceed {QueryDocumentsCommand.MaximumPageTokenLength} characters.",
                nameof(request.PageToken));
        }

        if (request.Filters?.Any(filter => filter is null) == true)
        {
            throw new ArgumentException(
                "A query filter cannot be null.",
                nameof(request.Filters));
        }

        if (request.Sorts?.Any(sort => sort is null) == true)
        {
            throw new ArgumentException(
                "A sort definition cannot be null.",
                nameof(request.Sorts));
        }
    }

    private static string CreateQueryFingerprint(QueryDocumentsCommand request)
    {
        var filters = (request.Filters ?? Array.Empty<QueryFilter>())
            .Select(filter => new
            {
                Field = filter.Field?.Trim(),
                Operator = filter.Operator.ToString(),
                Value = NormalizeFilterValue(filter.Value)
            })
            .ToArray();

        var fingerprintInput = new
        {
            Search = request.Search?.Trim(),
            Filters = filters
        };

        return Convert.ToHexString(
            SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(fingerprintInput)));
    }

    private static JsonElement NormalizeFilterValue(object? value)
    {
        return value switch
        {
            JsonElement element => element.Clone(),
            JsonDocument document => document.RootElement.Clone(),
            _ => JsonSerializer.SerializeToElement(value)
        };
    }
}
