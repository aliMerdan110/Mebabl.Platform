using System.Text;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.Documents.BatchWriteDocuments;

/// <summary>
/// Executes a bounded set of document mutations atomically inside one
/// project-owned collection. Every mutation is authorized and version checked.
/// </summary>
public sealed class BatchWriteDocumentsCommandHandler
    : IRequestHandler<BatchWriteDocumentsCommand, BatchWriteDocumentsResponse>
{
    private const int MaximumKeyLength = 200;

    private static readonly BatchWriteDocumentsCommandValidator CommandValidator = new();

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;
    private readonly IClock _clock;

    public BatchWriteDocumentsCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security,
        IClock clock)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser
            ?? throw new ArgumentNullException(nameof(currentUser));
        _security = security
            ?? throw new ArgumentNullException(nameof(security));
        _clock = clock
            ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<BatchWriteDocumentsResponse> Handle(
        BatchWriteDocumentsCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        CommandValidator.ValidateAndThrow(request);

        var (projectId, applicationId, userId) = GetAuthenticatedScope();

        var normalizedOperations = NormalizeOperations(request.Operations);

        // A batch cannot target the same document more than once. This also
        // prevents ambiguous ordering and multiple version increments.
        var targetDocumentIds = normalizedOperations
            .Where(operation => operation.DocumentId.HasValue)
            .Select(operation => operation.DocumentId!.Value)
            .ToArray();

        if (targetDocumentIds.Distinct().Count() != targetDocumentIds.Length)
        {
            throw new ArgumentException(
                "A document can be targeted only once in a batch.",
                nameof(request.Operations));
        }

        var hasWrites = normalizedOperations.Any(operation =>
            operation.Type is BatchWriteOperationType.Create or BatchWriteOperationType.Update);
        var hasDeletes = normalizedOperations.Any(operation =>
            operation.Type == BatchWriteOperationType.Delete);

        await using var transaction = await _dbContext.BeginTransactionAsync(
            cancellationToken);

        var ownedJsonDocuments = new List<JsonDocument>();
        var replacedJsonDocuments = new List<JsonDocument>();
        var committed = false;

        try
        {
            var collection = await _dbContext.Collections
                .AsNoTracking()
                .Where(collection =>
                    collection.Id == request.CollectionId &&
                    collection.ProjectId == projectId &&
                    collection.IsActive &&
                    !collection.IsDeleted &&
                    collection.Project.IsActive &&
                    !collection.Project.IsDeleted)
                .Select(collection => new
                {
                    collection.Id,
                    collection.ProjectId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (collection is null)
            {
                throw new KeyNotFoundException("Collection not found.");
            }

            if (hasWrites)
            {
                await _security.EnsureWriteAsync(
                    collection.Id,
                    cancellationToken);
            }

            if (hasDeletes)
            {
                await _security.EnsureDeleteAsync(
                    collection.Id,
                    cancellationToken);
            }

            var documentsById = new Dictionary<Guid, Document>();
            if (targetDocumentIds.Length > 0)
            {
                var documents = await _dbContext.Documents
                    .Where(document =>
                        targetDocumentIds.Contains(document.Id) &&
                        document.CollectionId == collection.Id &&
                        document.ProjectId == projectId &&
                        !document.IsDeleted &&
                        document.Collection.ProjectId == projectId &&
                        document.Collection.IsActive &&
                        !document.Collection.IsDeleted &&
                        document.Project.IsActive &&
                        !document.Project.IsDeleted)
                    .ToListAsync(cancellationToken);

                documentsById = documents.ToDictionary(document => document.Id);

                if (documentsById.Count != targetDocumentIds.Length)
                {
                    // Avoid revealing whether an unknown ID exists outside
                    // the authenticated project's collection.
                    throw new KeyNotFoundException("One or more documents were not found.");
                }
            }

            ValidateExpectedVersions(normalizedOperations, documentsById);
            await EnsureKeysAvailableAsync(
                collection.Id,
                projectId,
                normalizedOperations,
                cancellationToken);

            var now = _clock.UtcNow;
            var results = new List<BatchWriteDocumentResult>(normalizedOperations.Count);

            for (var index = 0; index < normalizedOperations.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var operation = normalizedOperations[index];
                switch (operation.Type)
                {
                    case BatchWriteOperationType.Create:
                    {
                        var data = CreateData(operation.Data!.Value);
                        ownedJsonDocuments.Add(data);

                        var document = new Document
                        {
                            Id = operation.GeneratedDocumentId,
                            ProjectId = collection.ProjectId,
                            CollectionId = collection.Id,
                            CreatedByApplicationId = applicationId,
                            CreatedByUserId = userId,
                            Key = operation.Key!,
                            Data = data,
                            Version = 1,
                            IsDeleted = false,
                            CreatedAt = now
                        };

                        _dbContext.Documents.Add(document);

                        results.Add(new BatchWriteDocumentResult(
                            index,
                            operation.Type,
                            document.Id,
                            document.Version,
                            document.IsDeleted));
                        break;
                    }

                    case BatchWriteOperationType.Update:
                    {
                        var document = documentsById[operation.DocumentId!.Value];
                        EnsureVersionCanAdvance(document);

                        var updatedData = CreateData(operation.Data!.Value);
                        ownedJsonDocuments.Add(updatedData);
                        replacedJsonDocuments.Add(document.Data);

                        document.Key = operation.Key!;
                        document.Data = updatedData;
                        document.Version++;
                        document.UpdatedAt = now;

                        results.Add(new BatchWriteDocumentResult(
                            index,
                            operation.Type,
                            document.Id,
                            document.Version,
                            document.IsDeleted));
                        break;
                    }

                    case BatchWriteOperationType.Delete:
                    {
                        var document = documentsById[operation.DocumentId!.Value];
                        EnsureVersionCanAdvance(document);

                        document.IsDeleted = true;
                        document.Version++;
                        document.UpdatedAt = now;

                        results.Add(new BatchWriteDocumentResult(
                            index,
                            operation.Type,
                            document.Id,
                            document.Version,
                            document.IsDeleted));
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(operation.Type),
                            operation.Type,
                            "Unsupported batch operation type.");
                }
            }

            // One SaveChanges call plus one transaction guarantees that no
            // partial batch is committed if any database command fails.
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            committed = true;

            foreach (var oldData in replacedJsonDocuments)
            {
                oldData?.Dispose();
            }

            return new BatchWriteDocumentsResponse(collection.Id, results);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackSafelyAsync(transaction);
            throw new InvalidOperationException(
                "One or more documents were modified by another request. Reload the affected documents and retry the batch.",
                exception);
        }
        catch
        {
            await RollbackSafelyAsync(transaction);
            throw;
        }
        finally
        {
            if (!committed)
            {
                foreach (var data in ownedJsonDocuments)
                {
                    data.Dispose();
                }
            }
        }
    }

    private (Guid ProjectId, Guid ApplicationId, Guid UserId) GetAuthenticatedScope()
    {
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

        return (projectId, applicationId, userId);
    }

    private static IReadOnlyList<NormalizedBatchOperation> NormalizeOperations(
        IReadOnlyList<BatchWriteDocumentOperation> operations)
    {
        var normalized = new List<NormalizedBatchOperation>(operations.Count);
        var totalPayloadBytes = 0;

        for (var index = 0; index < operations.Count; index++)
        {
            var operation = operations[index]
                ?? throw new ArgumentException(
                    $"Batch operation at index {index} cannot be null.",
                    nameof(operations));

            if (!Enum.IsDefined(operation.Type))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(operation.Type),
                    operation.Type,
                    $"Batch operation at index {index} has an unsupported type.");
            }

            switch (operation.Type)
            {
                case BatchWriteOperationType.Create:
                {
                    if (operation.DocumentId.HasValue || operation.ExpectedVersion.HasValue)
                    {
                        throw new ArgumentException(
                            $"Create operation at index {index} cannot specify DocumentId or ExpectedVersion.",
                            nameof(operations));
                    }

                    var key = NormalizeKey(operation.Key);
                    var data = ValidateAndGetData(operation.Data, index);
                    totalPayloadBytes = AddPayloadBytes(totalPayloadBytes, data);

                    normalized.Add(new NormalizedBatchOperation(
                        operation.Type,
                        key,
                        data,
                        null,
                        null,
                        Guid.NewGuid()));
                    break;
                }

                case BatchWriteOperationType.Update:
                {
                    var documentId = RequireDocumentId(operation.DocumentId, index);
                    var expectedVersion = RequireExpectedVersion(operation.ExpectedVersion, index);
                    var key = NormalizeKey(operation.Key);
                    var data = ValidateAndGetData(operation.Data, index);
                    totalPayloadBytes = AddPayloadBytes(totalPayloadBytes, data);

                    normalized.Add(new NormalizedBatchOperation(
                        operation.Type,
                        key,
                        data,
                        documentId,
                        expectedVersion,
                        Guid.Empty));
                    break;
                }

                case BatchWriteOperationType.Delete:
                {
                    var documentId = RequireDocumentId(operation.DocumentId, index);
                    var expectedVersion = RequireExpectedVersion(operation.ExpectedVersion, index);

                    if (operation.Key is not null || operation.Data.HasValue)
                    {
                        throw new ArgumentException(
                            $"Delete operation at index {index} cannot specify Key or Data.",
                            nameof(operations));
                    }

                    normalized.Add(new NormalizedBatchOperation(
                        operation.Type,
                        null,
                        null,
                        documentId,
                        expectedVersion,
                        Guid.Empty));
                    break;
                }
            }
        }

        return normalized;
    }

    private static int AddPayloadBytes(int currentBytes, JsonElement data)
    {
        var bytes = Encoding.UTF8.GetByteCount(data.GetRawText());
        var total = (long)currentBytes + bytes;

        if (total > BatchWriteDocumentsCommand.MaximumPayloadBytes)
        {
            throw new ArgumentException(
                $"Aggregate batch data cannot exceed {BatchWriteDocumentsCommand.MaximumPayloadBytes} UTF-8 bytes.");
        }

        return (int)total;
    }

    private static JsonElement ValidateAndGetData(
        JsonElement? data,
        int operationIndex)
    {
        if (!data.HasValue || data.Value.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException(
                $"Data for batch operation at index {operationIndex} must be a JSON object.");
        }

        return data.Value;
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Document key is required.");
        }

        var key = value.Trim();
        if (key.Length > MaximumKeyLength || key.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"Document key cannot exceed {MaximumKeyLength} characters or contain control characters.");
        }

        return key;
    }

    private static Guid RequireDocumentId(Guid? value, int operationIndex)
    {
        if (!value.HasValue || value.Value == Guid.Empty)
        {
            throw new ArgumentException(
                $"A valid DocumentId is required at batch operation index {operationIndex}.");
        }

        return value.Value;
    }

    private static int RequireExpectedVersion(int? value, int operationIndex)
    {
        if (!value.HasValue || value.Value < 1)
        {
            throw new ArgumentException(
                $"A positive ExpectedVersion is required at batch operation index {operationIndex}.");
        }

        return value.Value;
    }

    private static void ValidateExpectedVersions(
        IReadOnlyList<NormalizedBatchOperation> operations,
        IReadOnlyDictionary<Guid, Document> documents)
    {
        foreach (var operation in operations)
        {
            if (!operation.DocumentId.HasValue)
            {
                continue;
            }

            var document = documents[operation.DocumentId.Value];
            if (document.Version != operation.ExpectedVersion)
            {
                throw new InvalidOperationException(
                    $"Document version conflict for '{document.Id}'. Reload the document before retrying the batch.");
            }

            EnsureVersionCanAdvance(document);
        }
    }

    private async Task EnsureKeysAvailableAsync(
        Guid collectionId,
        Guid projectId,
        IReadOnlyList<NormalizedBatchOperation> operations,
        CancellationToken cancellationToken)
    {
        var keyIntents = operations
            .Where(operation => operation.Type is BatchWriteOperationType.Create or BatchWriteOperationType.Update)
            .Select(operation => new
            {
                Key = operation.Key!,
                IntendedDocumentId = operation.Type == BatchWriteOperationType.Create
                    ? operation.GeneratedDocumentId
                    : operation.DocumentId!.Value
            })
            .ToArray();

        if (keyIntents.Length == 0)
        {
            return;
        }

        var duplicateDesiredKeys = keyIntents
            .GroupBy(intent => intent.Key, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateDesiredKeys is not null)
        {
            throw new InvalidOperationException(
                $"The batch contains more than one document with key '{duplicateDesiredKeys.Key}'.");
        }

        var desiredKeys = keyIntents
            .Select(intent => intent.Key)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var existingKeys = await _dbContext.Documents
            .AsNoTracking()
            .Where(document =>
                document.ProjectId == projectId &&
                document.CollectionId == collectionId &&
                !document.IsDeleted &&
                desiredKeys.Contains(document.Key))
            .Select(document => new
            {
                document.Id,
                document.Key
            })
            .ToListAsync(cancellationToken);

        var intentByKey = keyIntents.ToDictionary(
            intent => intent.Key,
            intent => intent.IntendedDocumentId,
            StringComparer.Ordinal);

        foreach (var existing in existingKeys)
        {
            if (!intentByKey.TryGetValue(existing.Key, out var intendedDocumentId) ||
                intendedDocumentId != existing.Id)
            {
                throw new InvalidOperationException(
                    $"A document with key '{existing.Key}' already exists in the collection.");
            }
        }
    }

    private static void EnsureVersionCanAdvance(Document document)
    {
        if (document.Version == int.MaxValue)
        {
            throw new InvalidOperationException(
                $"Document '{document.Id}' has reached the maximum supported version.");
        }
    }

    private static JsonDocument CreateData(JsonElement element) =>
        JsonDocument.Parse(element.GetRawText());

    private static async Task RollbackSafelyAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch
        {
            // Preserve the original operation failure. Disposing the
            // transaction remains the final rollback safeguard.
        }
    }

    private sealed record NormalizedBatchOperation(
        BatchWriteOperationType Type,
        string? Key,
        JsonElement? Data,
        Guid? DocumentId,
        int? ExpectedVersion,
        Guid GeneratedDocumentId);
}
