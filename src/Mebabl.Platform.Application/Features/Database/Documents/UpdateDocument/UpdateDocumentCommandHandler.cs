using MediatR;
using Microsoft.EntityFrameworkCore;
using Mebabl.Platform.Application.Common.Interfaces;
using Mebabl.Platform.Application.Common.Security;

namespace Mebabl.Platform.Application.Features.Database.Documents.UpdateDocument;

public sealed class UpdateDocumentCommandHandler
    : IRequestHandler<UpdateDocumentCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly IDocumentSecurityService _security;

    public UpdateDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IDocumentSecurityService security)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _security = security;
    }

    public async Task Handle(
        UpdateDocumentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.ApplicationId == Guid.Empty ||
            _currentUser.UserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        await _security.EnsureWriteAsync(
            request.CollectionId,
            cancellationToken);

        var document = await _dbContext.Documents
            .FirstOrDefaultAsync(
                x =>
                    x.Id == request.DocumentId &&
                    x.CollectionId == request.CollectionId &&
                    x.ApplicationId == _currentUser.ApplicationId &&
                    !x.IsDeleted,
                cancellationToken);

        if (document is null)
            throw new KeyNotFoundException("Document not found.");

        if (document.UserId != _currentUser.UserId)
            throw new UnauthorizedAccessException();

        if (document.Version != request.ExpectedVersion)
            throw new InvalidOperationException(
                $"Document version conflict. Expected {request.ExpectedVersion}, current version is {document.Version}.");

        document.Key = request.Key.Trim();

        document.Data = System.Text.Json.JsonDocument.Parse(
            request.Data.RootElement.GetRawText());

        document.Version++;
        document.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}