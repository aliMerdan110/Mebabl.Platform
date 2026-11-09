using MediatR;
using Microsoft.EntityFrameworkCore;

using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Features.Database.Collections.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Collections.UpdateCollection;

/// <summary>
/// Updates collection metadata inside the project resolved from the
/// authenticated application. ProjectId is the resource boundary; the
/// calling ApplicationId is used only to resolve that trusted project.
/// </summary>
public sealed class UpdateCollectionCommandHandler
    : IRequestHandler<UpdateCollectionCommand, CollectionResponse>
{
    private const int MaximumNameLength = 100;
    private const int MaximumDescriptionLength = 500;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;

    public UpdateCollectionCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication)
    {
        _dbContext = dbContext
            ?? throw new ArgumentNullException(nameof(dbContext));
        _currentApplication = currentApplication
            ?? throw new ArgumentNullException(nameof(currentApplication));
    }

    public async Task<CollectionResponse> Handle(
        UpdateCollectionCommand request,
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

        if (request.CollectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "Collection ID is required.",
                nameof(request.CollectionId));
        }

        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);

        // Resolve the project from persisted application membership. Never
        // accept ProjectId from the request or trust a caller-supplied value.
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

        if (applicationScope is null ||
            applicationScope.ProjectId == Guid.Empty)
        {
            throw new UnauthorizedAccessException(
                "The application or its project is unavailable.");
        }

        var projectId = applicationScope.ProjectId;

        var collection = await _dbContext.Collections
            .FirstOrDefaultAsync(collection =>
                    collection.Id == request.CollectionId &&
                    collection.ProjectId == projectId &&
                    collection.IsActive &&
                    !collection.IsDeleted,
                cancellationToken);

        if (collection is null)
        {
            // Do not disclose collection identifiers belonging to another project.
            throw new KeyNotFoundException("Collection not found.");
        }

        if (collection.IsSystem)
        {
            throw new InvalidOperationException(
                "System collections cannot be modified through this operation.");
        }

        // Code is the stable collection identifier used by SDK paths and
        // references. Renaming the display name must not silently change it.
        collection.Name = name;
        collection.Description = description;

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
}
