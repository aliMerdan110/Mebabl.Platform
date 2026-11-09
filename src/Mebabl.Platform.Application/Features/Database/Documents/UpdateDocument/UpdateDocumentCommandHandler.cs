using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Services.Clock;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

/// <summary>
/// Replaces a project-scoped document using optimistic concurrency.
/// Authorization is determined by the published collection policy.
/// </summary>
public sealed class UpdateDocumentCommandHandler
    : IRequestHandler<UpdateDocumentCommand>
{
    private const int MaximumKeyLength = 200;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;
    private readonly IClock _clock;

    public UpdateDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security,
        IClock clock)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _security = security ?? throw new ArgumentNullException(nameof(security));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task Handle(
        UpdateDocumentCommand request,
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

        if (request.CollectionId == Guid.Empty || request.DocumentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID and document ID are required.");
        }

        if (request.ExpectedVersion < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.ExpectedVersion),
                "Expected version must be a positive integer.");
        }

        var key = NormalizeKey(request.Key);
        ValidateData(request.Data);

        var document = await _dbContext.Documents
            .Where(document =>
                document.Id == request.DocumentId &&
                document.CollectionId == request.CollectionId &&
                document.ProjectId == projectId &&
                !document.IsDeleted &&
                document.Collection.ProjectId == projectId &&
                document.Collection.IsActive &&
                !document.Collection.IsDeleted &&
                document.Project.IsActive &&
                !document.Project.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        await _security.EnsureWriteAsync(document.CollectionId, cancellationToken);

        if (document.Version != request.ExpectedVersion)
        {
            throw new InvalidOperationException(
                "Document version conflict. Reload the document before retrying.");
        }

        if (document.Version == int.MaxValue)
        {
            throw new InvalidOperationException(
                "The document version limit has been reached.");
        }

        var keyAlreadyExists = await _dbContext.Documents
            .AsNoTracking()
            .AnyAsync(other =>
                other.Id != document.Id &&
                other.ProjectId == projectId &&
                other.CollectionId == document.CollectionId &&
                other.Key == key &&
                !other.IsDeleted,
                cancellationToken);

        if (keyAlreadyExists)
        {
            throw new InvalidOperationException(
                "A document with this key already exists in the collection.");
        }

        var updatedData = CloneData(request.Data!);
        var previousData = document.Data;

        document.Key = key;
        document.Data = updatedData;
        document.Version++;
        document.UpdatedAt = _clock.UtcNow;

        try
        {
            // Version is an EF concurrency token. SaveChanges includes the
            // originally loaded value in the UPDATE predicate.
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            updatedData.Dispose();
            throw new InvalidOperationException(
                "Document was modified by another request. Reload it and retry.",
                exception);
        }
        catch
        {
            updatedData.Dispose();
            throw;
        }

        previousData?.Dispose();
    }

    private static string NormalizeKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Document key is required.",
                nameof(value));
        }

        var key = value.Trim();
        if (key.Length > MaximumKeyLength || key.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"Document key cannot exceed {MaximumKeyLength} characters or contain control characters.",
                nameof(value));
        }

        return key;
    }

    private static void ValidateData(JsonDocument? data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException(
                "Document data must be a JSON object.",
                nameof(data));
        }
    }

    private static JsonDocument CloneData(JsonDocument data) =>
        JsonDocument.Parse(data.RootElement.GetRawText());
}
