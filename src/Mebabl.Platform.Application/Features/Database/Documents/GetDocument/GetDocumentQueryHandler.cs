using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Application.Features.Database.Documents.GetDocument;

public sealed class GetDocumentQueryHandler
    : IRequestHandler<GetDocumentQuery, GetDocumentResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public GetDocumentQueryHandler(
        IApplicationDbContext context,
        ICurrentUser currentUser,
        IDocumentSecurityService security)
    {
        _context = context;
        _currentUser = currentUser;
        _security = security;
    }

    public async Task<GetDocumentResponse> Handle(
        GetDocumentQuery request,
        CancellationToken cancellationToken)
    {
        var document = await _context.Documents
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.Id &&
                    x.Collection.ApplicationId == _currentUser.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (document is null)
            throw new Exception("Document not found.");

        await _security.EnsureReadAsync(
            document.CollectionId,
            cancellationToken);

        return new GetDocumentResponse(
            document.Id,
            document.Key,
            document.Data,
            document.Version,
            document.CreatedAt,
            document.UpdatedAt);
    }
}