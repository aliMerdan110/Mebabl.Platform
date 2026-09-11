using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.ListDocuments;

public sealed class ListDocumentsQueryHandler
    : IRequestHandler<ListDocumentsQuery, IReadOnlyList<DocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IDocumentSecurityService _security;

    public ListDocumentsQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _security = security;
    }

    public async Task<IReadOnlyList<DocumentResponse>> Handle(
        ListDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated ||
            _currentApplication.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var collectionExists = await _dbContext.Collections
            .AnyAsync(
                x =>
                    x.Id == request.CollectionId &&
                    x.ApplicationId == _currentApplication.ApplicationId &&
                    x.IsActive,
                cancellationToken);

        if (!collectionExists)
            throw new KeyNotFoundException("Collection not found.");

        await _security.EnsureReadAsync(
            request.CollectionId,
            cancellationToken);

        return await _dbContext.Documents
            .AsNoTracking()
            .Where(x =>
                x.CollectionId == request.CollectionId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DocumentResponse(
                x.Id,
                x.Key,
                x.Data,
                x.Version,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}