using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;
using Mebabl.Platform.Application.Features.Database.Documents.DTOs;

namespace Mebabl.Platform.Application.Features.Database.Documents.GetDocument;

public sealed class GetDocumentQueryHandler
    : IRequestHandler<GetDocumentQuery, DocumentResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentApplication _currentApplication;
    private readonly IDocumentSecurityService _security;

    public GetDocumentQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentApplication currentApplication,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext;
        _currentApplication = currentApplication;
        _security = security;
    }

    public async Task<DocumentResponse> Handle(
        GetDocumentQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentApplication.IsAuthenticated ||
            _currentApplication.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var document = await _dbContext.Documents
            .AsNoTracking()
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.DocumentId &&
                    x.Collection.ApplicationId ==
                        _currentApplication.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (document is null)
            throw new KeyNotFoundException("Document not found.");

        await _security.EnsureReadAsync(
            document.CollectionId,
            cancellationToken);

        return new DocumentResponse(
            document.Id,
            document.Key,
            document.Data,
            document.Version,
            document.CreatedAt,
            document.UpdatedAt);
    }
}