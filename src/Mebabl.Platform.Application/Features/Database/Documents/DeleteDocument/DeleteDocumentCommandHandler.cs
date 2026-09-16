using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Application.Features.Database.Documents.DeleteDocument;

public sealed class DeleteDocumentCommandHandler
    : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public DeleteDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _security = security;
    }

    public async Task Handle(
        DeleteDocumentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        var document = await _dbContext.Documents
            .Include(x => x.Collection)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.DocumentId &&
                    x.CollectionId == request.CollectionId &&
                    x.Collection.ApplicationId == _currentUser.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (document is null)
            throw new KeyNotFoundException("Document not found.");

        await _security.EnsureDeleteAsync(
            request.CollectionId,
            cancellationToken);

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}