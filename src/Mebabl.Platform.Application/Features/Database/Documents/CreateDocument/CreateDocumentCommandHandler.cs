using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;
using Mebabl.Platform.Application.Services.Clock;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.Documents.CreateDocument;

/// <summary>
/// Creates a document in a project-owned collection. Project and provenance
/// identifiers are derived from the authenticated context, never the payload.
/// </summary>
public sealed class CreateDocumentCommandHandler
    : IRequestHandler<CreateDocumentCommand, DocumentResponse>
{
    private const int MaximumKeyLength = 200;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;
    private readonly IClock _clock;

    public CreateDocumentCommandHandler(
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

    public async Task<DocumentResponse> Handle(
        CreateDocumentCommand request,
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

        if (request.CollectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(request.CollectionId));
        }

        var key = NormalizeKey(request.Key);
        ValidateData(request.Data);

        // All applications in a project share the same collection namespace.
        // ApplicationId is retained only as creator/provenance metadata.
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

        await _security.EnsureWriteAsync(collection.Id, cancellationToken);

        var keyAlreadyExists = await _dbContext.Documents
            .AsNoTracking()
            .AnyAsync(document =>
                document.ProjectId == projectId &&
                document.CollectionId == collection.Id &&
                document.Key == key &&
                !document.IsDeleted,
                cancellationToken);

        if (keyAlreadyExists)
        {
            throw new InvalidOperationException(
                "A document with this key already exists in the collection.");
        }

        var data = CloneData(request.Data!);
        var document = new Document
        {
            Id = Guid.NewGuid(),
            ProjectId = collection.ProjectId,
            CollectionId = collection.Id,
            CreatedByApplicationId = applicationId,
            CreatedByUserId = userId,
            Key = key,
            Data = data,
            Version = 1,
            IsDeleted = false,
            CreatedAt = _clock.UtcNow
        };

        _dbContext.Documents.Add(document);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            data.Dispose();
            throw;
        }

        return new DocumentResponse(
            document.Id,
            document.Key,
            document.Data,
            document.Version,
            document.CreatedAt,
            document.UpdatedAt);
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
