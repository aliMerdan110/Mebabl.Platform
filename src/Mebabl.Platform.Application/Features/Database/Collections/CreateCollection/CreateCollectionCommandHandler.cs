using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;
using Mebabl.Platform.Domain.Entities.Database;

namespace Mebabl.Platform.Application.Features.Database.Collections.CreateCollection;

/// <summary>
/// Creates a collection in the authenticated application's project database.
/// The application authenticates the caller; ProjectId is the data-isolation boundary.
/// </summary>
public sealed class CreateCollectionCommandHandler
    : IRequestHandler<CreateCollectionCommand, CollectionResponse>
{
    private const int MaximumNameLength = 100;
    private const int MaximumDescriptionLength = 500;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public CreateCollectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentApplication = currentApplication
            ?? throw new ArgumentNullException(nameof(currentApplication));
    }

    public async Task<CollectionResponse> Handle(
        CreateCollectionCommand request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentApplication.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "An authenticated trusted application is required.");
        }

        var applicationId = _currentApplication.ApplicationId;
        if (applicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The authenticated application context is invalid.");
        }

        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);

        // Resolve project ownership from the trusted application record.
        // ProjectId is never accepted from the client request.
        var applicationScope = await _dbContext.Applications
            .AsNoTracking()
            .Where(application =>
                application.Id == applicationId &&
                application.IsActive &&
                !application.IsDeleted &&
                application.Project.IsActive &&
                !application.Project.IsDeleted)
            .Select(application => new
            {
                application.Id,
                application.ProjectId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (applicationScope is null || applicationScope.ProjectId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The application or its project is unavailable.");
        }

        var projectId = applicationScope.ProjectId;
        var code = GenerateCode(name);

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "Collection name must contain at least one letter or number.",
                nameof(request.Name));
        }

        var nameExists = await _dbContext.Collections
            .AsNoTracking()
            .AnyAsync(collection =>
                    collection.ProjectId == projectId &&
                    !collection.IsDeleted &&
                    collection.Name.ToLower() == name.ToLower(),
                cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "A collection with this name already exists in the project.");
        }

        var codeExists = await _dbContext.Collections
            .AsNoTracking()
            .AnyAsync(collection =>
                    collection.ProjectId == projectId &&
                    !collection.IsDeleted &&
                    collection.Code == code,
                cancellationToken);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "A collection with this code already exists in the project.");
        }

        var collection = new Collection
        {
            ProjectId = projectId,
            Name = name,
            Code = code,
            Description = description,
            IsSystem = false,
            IsActive = true
        };

        // New collections have no published policy and remain inaccessible
        // until a valid policy is explicitly published.
        _dbContext.Collections.Add(collection);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CollectionResponse(
            collection.Id,
            collection.ProjectId,
            collection.Name,
            collection.Description,
            collection.IsActive,
            collection.CreatedAt,
            collection.UpdatedAt);
    }

    private static string NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Collection name is required.",
                nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > MaximumNameLength ||
            normalized.Any(char.IsControl))
        {
            throw new ArgumentException(
                $"Collection name cannot exceed {MaximumNameLength} characters or contain control characters.",
                nameof(value));
        }

        return normalized;
    }

    private static string NormalizeDescription(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length > MaximumDescriptionLength)
        {
            throw new ArgumentException(
                $"Collection description cannot exceed {MaximumDescriptionLength} characters.",
                nameof(value));
        }

        return normalized;
    }

    private static string GenerateCode(string name)
    {
        var characters = name
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '_')
            .ToArray();

        var code = new string(characters);
        while (code.Contains("__", StringComparison.Ordinal))
        {
            code = code.Replace("__", "_", StringComparison.Ordinal);
        }

        return code.Trim('_');
    }
}
